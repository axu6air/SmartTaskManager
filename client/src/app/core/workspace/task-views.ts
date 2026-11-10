import { Injectable, computed, inject } from '@angular/core';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { AuthService } from '@core/auth/auth.service';
import { Guid } from '@models/api';
import { ProjectTask, isClosed } from '@models/task';
import { DueInfo, dueInfo } from '@shared/utils/date';
import { WorkspaceStore } from './workspace.store';

/** A task with everything a list, card or drawer shows about it, computed once for every page. */
export interface TaskView {
  task: ProjectTask;
  projectName: string;
  assigneeName: string | null;
  isMine: boolean;
  closed: boolean;
  due: DueInfo;
  canChangeStatus: boolean;
  canEdit: boolean;
  canAssign: boolean;
  canDelete: boolean;
}

@Injectable({ providedIn: 'root' })
export class TaskViews {
  private readonly store = inject(WorkspaceStore);
  private readonly permissions = inject(PermissionService);
  private readonly auth = inject(AuthService);

  /** Builds views for any tasks (store tasks or a server page); reactive on store, user and grants. */
  readonly toViews = computed(() => {
    const me = this.auth.me()?.id;
    const projects = this.store.projectById();
    const people = this.store.people();
    const members = this.store.members();
    // Read so the views recompute when an admin changes this user's grants.
    this.auth.permissions();
    const now = new Date();

    return (tasks: readonly ProjectTask[]): TaskView[] =>
      tasks.map((task) => {
        const projectMembers = members[task.projectId] ?? [];
        const closed = isClosed(task.status);
        const can = (code: Parameters<PermissionService['canOnTask']>[0]) =>
          this.permissions.canOnTask(code, task, projectMembers);

        return {
          task,
          projectName: projects.get(task.projectId)?.name ?? 'Unknown project',
          assigneeName: task.assigneeId ? (people.get(task.assigneeId)?.fullName ?? 'Former member') : null,
          isMine: task.assigneeId !== null && task.assigneeId === me,
          closed,
          due: dueInfo(task.dueDate, closed, now),
          canChangeStatus: can(Permissions.Tasks.UpdateStatus),
          canEdit: can(Permissions.Tasks.Update),
          canAssign: can(Permissions.Tasks.Assign),
          canDelete: can(Permissions.Tasks.Delete),
        };
      });
  });

  readonly all = computed<TaskView[]>(() => this.toViews()(this.store.tasks()));

  readonly byId = computed(() => new Map(this.all().map((v) => [v.task.id, v])));

  /** Projects the user may add tasks to. */
  readonly creatableProjects = computed(() => {
    this.auth.permissions();
    const members = this.store.members();
    return this.store
      .projects()
      .filter((p) => this.permissions.canOnProject(Permissions.Tasks.Create, members[p.id] ?? []));
  });

  forProject(projectId: Guid): TaskView[] {
    return this.all().filter((v) => v.task.projectId === projectId);
  }
}
