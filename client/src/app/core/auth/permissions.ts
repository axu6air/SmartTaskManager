/** Mirrors SmartTaskManager.Application.Common.Security.Permissions. Never use raw codes elsewhere. */
export const Permissions = {
  Projects: {
    View: 'projects.view',
    Create: 'projects.create',
    Update: 'projects.update',
    Delete: 'projects.delete',
    ManageMembers: 'projects.manage_members',
  },
  Tasks: {
    View: 'tasks.view',
    Create: 'tasks.create',
    Update: 'tasks.update',
    UpdateStatus: 'tasks.update_status',
    Assign: 'tasks.assign',
    Delete: 'tasks.delete',
  },
  Users: {
    View: 'users.view',
    Manage: 'users.manage',
  },
  Roles: {
    View: 'roles.view',
    Manage: 'roles.manage',
  },
  Dashboard: {
    View: 'dashboard.view',
  },
  Ai: {
    EnhanceDescription: 'ai.enhance_description',
  },
} as const;

type Values<T> = T[keyof T];
export type PermissionCode = Values<{ [K in keyof typeof Permissions]: Values<(typeof Permissions)[K]> }>;
