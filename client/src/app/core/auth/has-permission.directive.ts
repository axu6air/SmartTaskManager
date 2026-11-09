import { Directive, TemplateRef, ViewContainerRef, effect, inject, input } from '@angular/core';
import { PermissionService } from './permission.service';
import { PermissionCode } from './permissions';

/**
 * Renders its template only when the user holds the permission (or ANY of several).
 * Re-evaluates when permissions change, e.g. after an admin edits the user’s role.
 *
 *   <button *appHasPermission="perms.Tasks.Create">New task</button>
 */
@Directive({ selector: '[appHasPermission]' })
export class HasPermissionDirective {
  private readonly permissions = inject(PermissionService);
  private readonly template = inject(TemplateRef<unknown>);
  private readonly container = inject(ViewContainerRef);

  readonly appHasPermission = input.required<PermissionCode | readonly PermissionCode[]>();

  constructor() {
    // Creating and clearing views is a DOM side effect, which is what effect() is for.
    effect(() => {
      const value = this.appHasPermission();
      const allowed = this.permissions.hasAny(typeof value === 'string' ? [value] : value);

      if (allowed && this.container.length === 0) {
        this.container.createEmbeddedView(this.template);
      } else if (!allowed) {
        this.container.clear();
      }
    });
  }
}
