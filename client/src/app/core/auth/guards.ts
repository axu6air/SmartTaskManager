import { inject } from '@angular/core';
import { CanMatchFn, Route, Router, UrlSegment } from '@angular/router';
import { ToastService } from '@core/ui/toast.service';
import { AuthService } from './auth.service';
import { PermissionService } from './permission.service';
import { PermissionCode } from './permissions';

export interface PermissionRouteData {
  /** The user needs ANY of these permissions. */
  permissions: readonly PermissionCode[];
}

/** Signed-in users only; others go to sign-in and come back afterwards. */
export const authGuard: CanMatchFn = (_route: Route, segments: UrlSegment[]) => {
  if (inject(AuthService).isAuthenticated()) {
    return true;
  }
  const returnUrl = '/' + segments.map((s) => s.path).join('/');
  return inject(Router).createUrlTree(['/login'], {
    queryParams: returnUrl === '/' ? {} : { returnUrl },
  });
};

/** Keeps signed-in users away from the sign-in page. */
export const guestGuard: CanMatchFn = () =>
  inject(AuthService).isAuthenticated() ? inject(Router).createUrlTree(['/']) : true;

/** canMatch, so a user without the permission never downloads the feature chunk. */
export const permissionGuard: CanMatchFn = (route: Route) => {
  const required = (route.data as PermissionRouteData | undefined)?.permissions ?? [];
  if (required.length === 0 || inject(PermissionService).hasAny(required)) {
    return true;
  }
  inject(ToastService).error('You don’t have permission to open that page.');
  return inject(Router).createUrlTree(['/']);
};
