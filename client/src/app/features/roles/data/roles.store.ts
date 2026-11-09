import { inject } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { toAppError } from '@core/http/app-error';
import { ToastService } from '@core/ui/toast.service';
import { Guid } from '@models/api';
import { Role, RolesMatrix } from '@models/role';
import { scopeLabel } from '@models/scope';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { RolesApi } from './roles-api';

interface RolesState {
  matrix: RolesMatrix | null;
  status: 'loading' | 'loaded' | 'error';
  error: string | null;
}

function withScope(role: Role, permissionId: Guid, scope: number): Role {
  const others = role.grants.filter((g) => g.permissionId !== permissionId);
  return { ...role, grants: scope === 0 ? others : [...others, { permissionId, scope }] };
}

/** Provided by the roles page itself, so it is created fresh per visit and never outlives a session. */
export const RolesStore = signalStore(
  withState<RolesState>({ matrix: null, status: 'loading', error: null }),
  withMethods((store) => {
    const api = inject(RolesApi);
    const auth = inject(AuthService);
    const toast = inject(ToastService);

    const replaceRole = (role: Role) => {
      const matrix = store.matrix();
      if (matrix) {
        patchState(store, { matrix: { ...matrix, roles: matrix.roles.map((r) => (r.id === role.id ? role : r)) } });
      }
    };

    return {
      async load(): Promise<void> {
        patchState(store, { status: 'loading', error: null });
        try {
          patchState(store, { matrix: await firstValueFrom(api.matrix()), status: 'loaded' });
        } catch (error) {
          patchState(store, { status: 'error', error: toAppError(error).message });
        }
      },

      /** Optimistic: 0 revokes. Rolls back if the server refuses. */
      async setScope(role: Role, permissionId: Guid, permissionName: string, scope: number): Promise<void> {
        replaceRole(withScope(role, permissionId, scope));
        try {
          if (scope === 0) {
            await firstValueFrom(api.revoke(role.id, permissionId));
            toast.success(`Revoked “${permissionName}” from ${role.name}`);
          } else {
            await firstValueFrom(api.grant(role.id, permissionId, scope));
            toast.success(`${role.name}: “${permissionName}” set to ${scopeLabel(scope)}`);
          }
          // The change may affect the signed-in user's own role: refresh so menus and actions update now.
          await auth.loadMe();
        } catch {
          replaceRole(role);
        }
      },
    };
  }),
);
