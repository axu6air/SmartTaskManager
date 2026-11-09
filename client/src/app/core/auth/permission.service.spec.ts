import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ProjectMember } from '@models/project';
import { PermissionScope } from '@models/scope';
import { AuthService } from './auth.service';
import { PermissionService } from './permission.service';
import { Permissions } from './permissions';

describe('PermissionService', () => {
  const permissions = signal<Record<string, number>>({});
  let service: PermissionService;

  const member = (userId: string, isManager = false): ProjectMember => ({
    userId,
    fullName: userId,
    email: `${userId}@example.com`,
    isManager,
  });

  beforeEach(() => {
    permissions.set({});
    TestBed.configureTestingModule({
      providers: [{ provide: AuthService, useValue: { permissions, me: signal({ id: 'me' }) } }],
    });
    service = TestBed.inject(PermissionService);
  });

  it('has / hasAny / hasAll read the held codes', () => {
    permissions.set({ [Permissions.Tasks.View]: PermissionScope.Member });

    expect(service.has(Permissions.Tasks.View)).toBe(true);
    expect(service.has(Permissions.Tasks.Delete)).toBe(false);
    expect(service.hasAny([Permissions.Tasks.Delete, Permissions.Tasks.View])).toBe(true);
    expect(service.hasAll([Permissions.Tasks.Delete, Permissions.Tasks.View])).toBe(false);
  });

  it('Assigned scope allows only tasks assigned to the user', () => {
    permissions.set({ [Permissions.Tasks.UpdateStatus]: PermissionScope.Assigned });
    const members = [member('me')];

    expect(service.canOnTask(Permissions.Tasks.UpdateStatus, { assigneeId: 'me' }, members)).toBe(true);
    expect(service.canOnTask(Permissions.Tasks.UpdateStatus, { assigneeId: 'other' }, members)).toBe(false);
  });

  it('Managed scope requires the user to manage the project', () => {
    permissions.set({ [Permissions.Tasks.Delete]: PermissionScope.Managed });

    expect(service.canOnTask(Permissions.Tasks.Delete, { assigneeId: null }, [member('me', true)])).toBe(true);
    expect(service.canOnTask(Permissions.Tasks.Delete, { assigneeId: null }, [member('me')])).toBe(false);
  });

  it('combined flags grant access when any condition holds', () => {
    permissions.set({ [Permissions.Tasks.UpdateStatus]: PermissionScope.Assigned | PermissionScope.Managed });

    expect(service.canOnTask(Permissions.Tasks.UpdateStatus, { assigneeId: 'me' }, [member('me')])).toBe(true);
    expect(service.canOnTask(Permissions.Tasks.UpdateStatus, { assigneeId: 'x' }, [member('me', true)])).toBe(true);
    expect(service.canOnTask(Permissions.Tasks.UpdateStatus, { assigneeId: 'x' }, [member('me')])).toBe(false);
  });

  it('Assigned never grants project-level actions', () => {
    permissions.set({ [Permissions.Tasks.Create]: PermissionScope.Assigned });

    expect(service.canOnProject(Permissions.Tasks.Create, [member('me')])).toBe(false);
  });

  it('All scope works without membership', () => {
    permissions.set({ [Permissions.Tasks.Create]: PermissionScope.All });

    expect(service.canOnProject(Permissions.Tasks.Create, [])).toBe(true);
  });
});
