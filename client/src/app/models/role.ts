import { Guid } from './api';

/** Mirrors RolesResponse (GET /api/roles). */
export interface RolesMatrix {
  modules: PermissionModule[];
  roles: Role[];
}

export interface PermissionModule {
  code: string;
  name: string;
  permissions: PermissionDefinition[];
}

export interface PermissionDefinition {
  id: Guid;
  code: string;
  name: string;
  isScopable: boolean;
}

export interface Role {
  id: Guid;
  name: string;
  description: string | null;
  isSystem: boolean;
  grants: RoleGrant[];
}

/** Mirrors RoleGrantResponse; scope is a PermissionScopes flags value. */
export interface RoleGrant {
  permissionId: Guid;
  scope: number;
}
