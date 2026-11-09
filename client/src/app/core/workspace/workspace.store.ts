import { computed, inject } from '@angular/core';
import { toAppError } from '@core/http/app-error';
import { Guid, PagedResponse } from '@models/api';
import { CreateProjectRequest, ProjectMember, ProjectSummary } from '@models/project';
import {
  CreateTaskRequest,
  ProjectTask,
  TaskStatus,
  UpdateTaskRequest,
} from '@models/task';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { Observable, firstValueFrom } from 'rxjs';
import { ProjectApi } from './project-api';
import { TaskApi } from './task-api';

const PAGE_SIZE = 100; // the API's maximum

interface WorkspaceState {
  projects: ProjectSummary[];
  members: Record<Guid, ProjectMember[]>;
  tasks: ProjectTask[];
  status: 'idle' | 'loading' | 'loaded' | 'error';
  error: string | null;
}

const initialState: WorkspaceState = {
  projects: [],
  members: {},
  tasks: [],
  status: 'idle',
  error: null,
};

async function fetchAll<T>(page: (n: number) => Observable<PagedResponse<T>>): Promise<T[]> {
  const items: T[] = [];
  for (let n = 1; ; n++) {
    const result = await firstValueFrom(page(n));
    items.push(...result.items);
    if (result.items.length === 0 || items.length >= result.totalCount) {
      return items;
    }
  }
}

function byName(projects: ProjectSummary[]): ProjectSummary[] {
  return [...projects].sort((a, b) => a.name.localeCompare(b.name));
}

/** Create/update return the full ProjectResponse; keep only the summary fields (members are stored separately). */
function toSummary(p: ProjectSummary): ProjectSummary {
  return { id: p.id, name: p.name, description: p.description, startDate: p.startDate, endDate: p.endDate };
}

/**
 * The projects the user can see, their members and their tasks, shared by the projects, board and
 * dashboard pages. Root-provided because three features read it; AuthService resets it on sign-out.
 */
export const WorkspaceStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ projects, members, tasks }) => ({
    projectById: computed(() => new Map(projects().map((p) => [p.id, p]))),
    /** Everyone in any visible project, so assignee names resolve on cross-project pages. */
    people: computed(() => {
      const map = new Map<Guid, ProjectMember>();
      Object.values(members()).forEach((list) => list.forEach((m) => map.set(m.userId, m)));
      return map;
    }),
    tasksByProject: computed(() => {
      const map = new Map<Guid, ProjectTask[]>();
      tasks().forEach((t) => map.set(t.projectId, [...(map.get(t.projectId) ?? []), t]));
      return map;
    }),
  })),
  withMethods((store) => {
    const projectApi = inject(ProjectApi);
    const taskApi = inject(TaskApi);
    // Bumped on reset, so a load that finishes after sign-out cannot write the old user's data back.
    let generation = 0;

    const replaceTask = (task: ProjectTask) =>
      patchState(store, { tasks: store.tasks().map((t) => (t.id === task.id ? task : t)) });

    return {
      // shortcut: 1 + 2 requests per project (members, tasks), fine for tens of projects; add a
      // cross-project task query on the API when workspaces grow past that.
      async load(options: { includeTasks: boolean }): Promise<void> {
        const run = ++generation;
        patchState(store, { status: 'loading', error: null });
        try {
          const projects = await fetchAll((n) => projectApi.list(n, PAGE_SIZE));
          const details = await Promise.all(
            projects.map((p) =>
              Promise.all([
                firstValueFrom(projectApi.members(p.id)),
                options.includeTasks
                  ? fetchAll((n) => taskApi.list(p.id, n, PAGE_SIZE))
                  : Promise.resolve([]),
              ]),
            ),
          );
          if (run !== generation) {
            return;
          }
          patchState(store, {
            projects: byName(projects),
            members: Object.fromEntries(projects.map((p, i) => [p.id, details[i][0]])),
            tasks: details.flatMap(([, tasks]) => tasks),
            status: 'loaded',
          });
        } catch (error) {
          if (run === generation) {
            patchState(store, { status: 'error', error: toAppError(error).message });
          }
        }
      },

      /** The creator becomes the first manager, so the member list is fetched rather than assumed. */
      async createProject(body: CreateProjectRequest): Promise<ProjectSummary> {
        const project = toSummary(await firstValueFrom(projectApi.create(body)));
        const members = await firstValueFrom(projectApi.members(project.id));
        patchState(store, {
          projects: byName([...store.projects(), project]),
          members: { ...store.members(), [project.id]: members },
        });
        return project;
      },

      async updateProject(projectId: Guid, body: CreateProjectRequest): Promise<void> {
        const project = toSummary(await firstValueFrom(projectApi.update(projectId, body)));
        patchState(store, {
          projects: byName(store.projects().map((p) => (p.id === projectId ? project : p))),
        });
      },

      async deleteProject(projectId: Guid): Promise<void> {
        await firstValueFrom(projectApi.delete(projectId));
        patchState(store, {
          projects: store.projects().filter((p) => p.id !== projectId),
          members: Object.fromEntries(Object.entries(store.members()).filter(([id]) => id !== projectId)),
          tasks: store.tasks().filter((t) => t.projectId !== projectId),
        });
      },

      /** Add a member or change their manager flag, then re-read the list (it carries the names). */
      async setMember(projectId: Guid, userId: Guid, isManager: boolean): Promise<void> {
        await firstValueFrom(projectApi.assignMember(projectId, userId, isManager));
        const members = await firstValueFrom(projectApi.members(projectId));
        patchState(store, { members: { ...store.members(), [projectId]: members } });
      },

      /** Mirrors the server: the member's tasks in this project become unassigned. */
      async removeMember(projectId: Guid, userId: Guid): Promise<void> {
        await firstValueFrom(projectApi.removeMember(projectId, userId));
        patchState(store, {
          members: {
            ...store.members(),
            [projectId]: (store.members()[projectId] ?? []).filter((m) => m.userId !== userId),
          },
          tasks: store
            .tasks()
            .map((t) => (t.projectId === projectId && t.assigneeId === userId ? { ...t, assigneeId: null } : t)),
        });
      },

      async createTask(projectId: Guid, body: CreateTaskRequest): Promise<ProjectTask> {
        const task = await firstValueFrom(taskApi.create(projectId, body));
        patchState(store, { tasks: [...store.tasks(), task] });
        return task;
      },

      /** Saves the fields, then the assignee if it changed (a separate permission and endpoint). */
      async updateTask(
        task: ProjectTask,
        body: UpdateTaskRequest,
        assigneeId: Guid | null | undefined,
      ): Promise<void> {
        let saved = await firstValueFrom(taskApi.update(task, body));
        replaceTask(saved);
        if (assigneeId !== undefined && assigneeId !== task.assigneeId) {
          saved = await firstValueFrom(taskApi.assign(saved, assigneeId));
          replaceTask(saved);
        }
      },

      /** Optimistic: the card moves at once and moves back if the server refuses. */
      async changeStatus(task: ProjectTask, status: TaskStatus): Promise<void> {
        if (task.status === status) {
          return;
        }
        replaceTask({ ...task, status });
        try {
          replaceTask(await firstValueFrom(taskApi.changeStatus(task, status)));
        } catch (error) {
          replaceTask(task);
          throw error;
        }
      },

      /** Optimistic, with the task restored in place on failure. */
      async deleteTask(task: ProjectTask): Promise<void> {
        patchState(store, { tasks: store.tasks().filter((t) => t.id !== task.id) });
        try {
          await firstValueFrom(taskApi.delete(task));
        } catch (error) {
          patchState(store, { tasks: [...store.tasks(), task] });
          throw error;
        }
      },

      reset(): void {
        generation++;
        patchState(store, initialState);
      },
    };
  }),
);
