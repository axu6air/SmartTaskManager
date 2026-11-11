import { Guid, OffsetDateString } from './api';

export interface LoginRequest {
  email: string;
  password: string;
}

/** Mirrors LoginResponse; also returned by POST /api/auth/refresh. */
export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: OffsetDateString;
}

/** Mirrors RegisterUserRequest (POST /api/auth/register, Manage Users permission). No roleId = default role. */
export interface RegisterUserRequest {
  fullName: string;
  email: string;
  password: string;
  roleId: Guid | null;
}

/** Mirrors RegisterUserResponse. */
export interface RegisteredUser {
  id: Guid;
  fullName: string;
  email: string;
  roles: string[];
}

/** Mirrors MeResponse: permissions map each held code to its PermissionScopes flags. */
export interface Me {
  id: Guid;
  fullName: string;
  email: string;
  roles: string[];
  permissions: Record<string, number>;
}
