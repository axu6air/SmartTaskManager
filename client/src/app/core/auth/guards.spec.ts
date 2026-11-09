import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Route, Router, UrlSegment, UrlTree, provideRouter } from '@angular/router';
import { ToastService } from '@core/ui/toast.service';
import { AuthService } from './auth.service';
import { authGuard, permissionGuard } from './guards';
import { Permissions } from './permissions';

describe('route guards', () => {
  const me = signal<{ id: string } | null>(null);
  const permissions = signal<Record<string, number>>({});
  const toast = { error: vi.fn() };

  beforeEach(() => {
    me.set(null);
    permissions.set({});
    toast.error.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { me, permissions, isAuthenticated: () => me() !== null } },
        { provide: ToastService, useValue: toast },
      ],
    });
  });

  const run = <T>(fn: () => T) => TestBed.runInInjectionContext(fn);

  it('authGuard sends signed-out users to sign-in with a return URL', () => {
    const result = run(() => authGuard({}, [new UrlSegment('board', {})])) as UrlTree;

    expect(TestBed.inject(Router).serializeUrl(result)).toBe('/login?returnUrl=%2Fboard');
  });

  it('authGuard lets signed-in users through', () => {
    me.set({ id: 'u1' });

    expect(run(() => authGuard({}, []))).toBe(true);
  });

  it('permissionGuard refuses users without any listed permission', () => {
    const route: Route = { data: { permissions: [Permissions.Roles.View] } };

    const result = run(() => permissionGuard(route, []));

    expect(result).toBeInstanceOf(UrlTree);
    expect(toast.error).toHaveBeenCalled();
  });

  it('permissionGuard allows users holding one of the permissions', () => {
    permissions.set({ [Permissions.Roles.View]: 8 });
    const route: Route = { data: { permissions: [Permissions.Roles.View, Permissions.Roles.Manage] } };

    expect(run(() => permissionGuard(route, []))).toBe(true);
  });
});
