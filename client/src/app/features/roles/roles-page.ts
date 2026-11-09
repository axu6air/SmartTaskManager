import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { PermissionDefinition, Role } from '@models/role';
import { PermissionScope, scopeLabel } from '@models/scope';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { RolesStore } from './data/roles.store';

/** Scope choices offered for scopable permissions; any other stored combination is shown as-is. */
const SCOPE_OPTIONS = [
  0,
  PermissionScope.Assigned,
  PermissionScope.Managed,
  PermissionScope.Assigned | PermissionScope.Managed,
  PermissionScope.Member,
  PermissionScope.All,
];

interface Cell {
  role: Role;
  scope: number;
  options: { value: number; label: string }[];
}

@Component({
  selector: 'app-roles-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, LoadError],
  providers: [RolesStore],
  templateUrl: './roles-page.html',
  styleUrl: './roles-page.scss',
})
export class RolesPage {
  protected readonly store = inject(RolesStore);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);

  protected readonly canManage = computed(() => {
    this.auth.permissions();
    return this.permissions.has(Permissions.Roles.Manage);
  });
  protected readonly myRoles = computed(() => new Set(this.auth.me()?.roles ?? []));
  protected readonly roles = computed(() => this.store.matrix()?.roles ?? []);

  protected readonly rows = computed(() => {
    const matrix = this.store.matrix();
    if (!matrix) {
      return [];
    }
    return matrix.modules.map((module) => ({
      ...module,
      permissions: module.permissions.map((permission) => ({
        permission,
        cells: matrix.roles.map((role) => this.cell(role, permission)),
      })),
    }));
  });

  constructor() {
    void this.store.load();
  }

  protected toggle(cell: Cell, permission: PermissionDefinition): void {
    void this.store.setScope(cell.role, permission.id, permission.name, cell.scope === 0 ? PermissionScope.All : 0);
  }

  protected async choose(cell: Cell, permission: PermissionDefinition, event: Event): Promise<void> {
    const select = event.target as HTMLSelectElement;
    await this.store.setScope(cell.role, permission.id, permission.name, Number(select.value));
    // After a rollback the bound state equals what it was before, so Angular won't touch the DOM;
    // put the element back in line with the store explicitly.
    const role = this.store.matrix()?.roles.find((r) => r.id === cell.role.id);
    select.value = String(role?.grants.find((g) => g.permissionId === permission.id)?.scope ?? 0);
  }

  private cell(role: Role, permission: PermissionDefinition): Cell {
    const scope = role.grants.find((g) => g.permissionId === permission.id)?.scope ?? 0;
    const values = SCOPE_OPTIONS.includes(scope) ? SCOPE_OPTIONS : [...SCOPE_OPTIONS, scope];
    return {
      role,
      scope,
      options: values.map((value) => ({ value, label: value === 0 ? 'No access' : scopeLabel(value) })),
    };
  }
}
