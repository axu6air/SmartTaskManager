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

/** Mirrors MeResponse: permissions map each held code to its PermissionScopes flags. */
export interface Me {
  id: Guid;
  fullName: string;
  email: string;
  roles: string[];
  permissions: Record<string, number>;
}
