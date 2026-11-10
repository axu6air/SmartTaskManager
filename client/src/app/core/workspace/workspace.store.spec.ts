import { TestBed } from '@angular/core/testing';
import { AppError } from '@core/http/app-error';
import { ProjectTask, TaskStatus } from '@models/task';
import { patchState } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { of, throwError } from 'rxjs';
import { ProjectApi } from './project-api';
import { TaskApi } from './task-api';
import { WorkspaceStore } from './workspace.store';

describe('WorkspaceStore', () => {
  const task: ProjectTask = {
    id: 't1',
    projectId: 'p1',
    title: 'Write tests',
    description: null,
    status: TaskStatus.ToDo,
    priority: 2,
    dueDate: null,
    assigneeId: null,
    completedAt: null,
    isAiModified: false,
  };
  const taskApi = { changeStatus: vi.fn(), delete: vi.fn() };

  function setup() {
    TestBed.configureTestingModule({ providers: [{ provide: TaskApi, useValue: taskApi }] });
    const store = TestBed.inject(WorkspaceStore);
    patchState(unprotected(store), { tasks: [task], status: 'loaded' });
    return store;
  }

  beforeEach(() => {
    taskApi.changeStatus.mockReset();
    taskApi.delete.mockReset();
  });

  it('changeStatus applies the server result', async () => {
    const store = setup();
    taskApi.changeStatus.mockReturnValue(of({ ...task, status: TaskStatus.Completed, completedAt: '2026-11-08T10:00:00+06:00' }));

    await store.changeStatus(task, TaskStatus.Completed);

    expect(store.tasks()[0].status).toBe(TaskStatus.Completed);
    expect(store.tasks()[0].completedAt).not.toBeNull();
  });

  it('changeStatus rolls back when the server refuses', async () => {
    const store = setup();
    taskApi.changeStatus.mockReturnValue(throwError(() => new AppError(403, 'No')));

    await expect(store.changeStatus(task, TaskStatus.InProgress)).rejects.toBeInstanceOf(AppError);

    expect(store.tasks()[0].status).toBe(TaskStatus.ToDo);
  });

  it('deleteTask restores the task when the server refuses', async () => {
    const store = setup();
    taskApi.delete.mockReturnValue(throwError(() => new AppError(404, 'Gone')));

    await expect(store.deleteTask(task)).rejects.toBeInstanceOf(AppError);

    expect(store.tasks().map((t) => t.id)).toEqual(['t1']);
  });

  it('createProject adds the project in name order with its fetched members', async () => {
    const projectApi = {
      create: vi.fn().mockReturnValue(
        of({ id: 'p0', name: 'Alpha', description: null, startDate: null, endDate: null, members: [{ userId: 'u1', isManager: true }] }),
      ),
      members: vi.fn().mockReturnValue(of([{ userId: 'u1', fullName: 'Me', email: 'me@x.com', isManager: true }])),
    };
    TestBed.overrideProvider(ProjectApi, { useValue: projectApi });
    const store = setup();
    patchState(unprotected(store), { projects: [{ id: 'p1', name: 'Zeta', description: null, startDate: null, endDate: null, isAiModified: false }] });

    await store.createProject({ name: 'Alpha', description: null, startDate: null, endDate: null });

    expect(store.projects().map((p) => p.name)).toEqual(['Alpha', 'Zeta']);
    expect(store.projects()[0]).not.toHaveProperty('members');
    expect(store.members()['p0'][0].isManager).toBe(true);
  });

  it('updateProject replaces the project and keeps name order', async () => {
    const projectApi = {
      update: vi.fn().mockReturnValue(
        of({ id: 'p1', name: 'Beta', description: 'New', startDate: null, endDate: null, members: [] }),
      ),
    };
    TestBed.overrideProvider(ProjectApi, { useValue: projectApi });
    const store = setup();
    patchState(unprotected(store), {
      projects: [
        { id: 'p1', name: 'Zeta', description: null, startDate: null, endDate: null, isAiModified: false },
        { id: 'p2', name: 'Gamma', description: null, startDate: null, endDate: null, isAiModified: false },
      ],
    });

    await store.updateProject('p1', { name: 'Beta', description: 'New', startDate: null, endDate: null, isAiModified: false });

    expect(store.projects().map((p) => p.name)).toEqual(['Beta', 'Gamma']);
    expect(store.projects()[0].description).toBe('New');
    expect(store.projects()[0]).not.toHaveProperty('members');
  });

  it('removeMember drops the member and unassigns only their tasks in that project', async () => {
    TestBed.overrideProvider(ProjectApi, { useValue: { removeMember: vi.fn().mockReturnValue(of(undefined)) } });
    const store = setup();
    const member = (userId: string) => ({ userId, fullName: userId, email: `${userId}@x.com`, isManager: false });
    patchState(unprotected(store), {
      members: { p1: [member('u1'), member('u2')], p2: [member('u1')] },
      tasks: [
        { ...task, id: 'a', assigneeId: 'u1' },
        { ...task, id: 'b', assigneeId: 'u2' },
        { ...task, id: 'c', projectId: 'p2', assigneeId: 'u1' },
      ],
    });

    await store.removeMember('p1', 'u1');

    expect(store.members()['p1'].map((m) => m.userId)).toEqual(['u2']);
    expect(store.tasks().map((t) => t.assigneeId)).toEqual([null, 'u2', 'u1']);
  });

  it('deleteProject removes the project, its members and its tasks', async () => {
    TestBed.overrideProvider(ProjectApi, { useValue: { delete: vi.fn().mockReturnValue(of(undefined)) } });
    const store = setup();
    patchState(unprotected(store), {
      projects: [
        { id: 'p1', name: 'One', description: null, startDate: null, endDate: null, isAiModified: false },
        { id: 'p2', name: 'Two', description: null, startDate: null, endDate: null, isAiModified: false },
      ],
      members: { p1: [], p2: [] },
      tasks: [task, { ...task, id: 't2', projectId: 'p2' }],
    });

    await store.deleteProject('p1');

    expect(store.projects().map((p) => p.id)).toEqual(['p2']);
    expect(Object.keys(store.members())).toEqual(['p2']);
    expect(store.tasks().map((t) => t.id)).toEqual(['t2']);
  });

  it('reset clears everything', () => {
    const store = setup();

    store.reset();

    expect(store.tasks()).toEqual([]);
    expect(store.status()).toBe('idle');
  });
});
