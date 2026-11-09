import { Injectable, inject } from '@angular/core';
import { Guid } from '@models/api';
import { ProjectMember } from '@models/project';
import { PermissionScope } from '@models/scope';
import { AuthService } from './auth.service';
import { PermissionCode } from './permissions';

/**
 * UX-only permission checks; the API is the source of truth.
 * Scope rules mirror the server’s ScopeFilters: access holds when ANY granted flag’s condition holds.
 */
@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly auth = inject(AuthService);

  has(code: PermissionCode): boolean {
    return this.scopeOf(code) !== 0;
  }

  hasAny(codes: readonly PermissionCode[]): boolean {
    return codes.some((code) => this.has(code));
  }

  hasAll(codes: readonly PermissionCode[]): boolean {
    return codes.every((code) => this.has(code));
  }

  /** 0 when the permission is not held. */
  scopeOf(code: PermissionCode): number {
    return this.auth.permissions()[code] ?? 0;
  }

  /** For project-level actions (create a task in it). The Assigned flag never applies to a project. */
  canOnProject(code: PermissionCode, members: readonly ProjectMember[]): boolean {
    const scope = this.scopeOf(code);
    const me = this.auth.me()?.id;
    const member = members.find((m) => m.userId === me);

    return (
      (scope & PermissionScope.All) !== 0 ||
      ((scope & PermissionScope.Managed) !== 0 && member?.isManager === true) ||
      ((scope & PermissionScope.Member) !== 0 && member !== undefined)
    );
  }

  /** For actions on one task (change status, edit, delete). */
  canOnTask(
    code: PermissionCode,
    task: { assigneeId: Guid | null },
    members: readonly ProjectMember[],
  ): boolean {
    const scope = this.scopeOf(code);
    const me = this.auth.me()?.id;

    return (
      ((scope & PermissionScope.Assigned) !== 0 && me !== undefined && task.assigneeId === me) ||
      this.canOnProject(code, members)
    );
  }
}
