import { Injectable, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { CanDeactivateFn, PreloadingStrategy, Route, RouterStateSnapshot, TitleStrategy } from '@angular/router';
import { ConfirmDialogService } from '@core/ui/confirm-dialog.service';
import { PermissionRouteData } from '@core/auth/guards';
import { PermissionService } from '@core/auth/permission.service';
import { Observable, EMPTY } from 'rxjs';
import { NAV_ITEMS } from './navigation';

/** Preloads a lazy route only when the user may open it, so restricted code never downloads. */
@Injectable({ providedIn: 'root' })
export class PermittedPreloading implements PreloadingStrategy {
  private readonly permissions = inject(PermissionService);

  preload(route: Route, load: () => Observable<unknown>): Observable<unknown> {
    const required = (route.data as PermissionRouteData | undefined)?.permissions ?? [];
    return required.length === 0 || this.permissions.hasAny(required) ? load() : EMPTY;
  }
}

/** "Projects · Smart Task Manager". */
@Injectable({ providedIn: 'root' })
export class AppTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const page = this.buildTitle(snapshot);
    this.title.setTitle(page ? `${page} · Smart Task Manager` : 'Smart Task Manager');
  }
}

/** For pages with a form: asks before leaving with unsaved changes. */
export const unsavedChangesGuard: CanDeactivateFn<{ hasUnsavedChanges(): boolean }> = (page) =>
  !page.hasUnsavedChanges() ||
  inject(ConfirmDialogService).confirm({
    title: 'Leave this page?',
    message: 'You have unsaved changes that will be lost.',
    confirmLabel: 'Leave',
    cancelLabel: 'Stay',
    danger: true,
  });

/** The first page in the menu this user may open, or the no-access page. */
export function homePath(): string {
  const permissions = inject(PermissionService);
  return NAV_ITEMS.find((item) => permissions.hasAny(item.permissions))?.path ?? '/no-access';
}
