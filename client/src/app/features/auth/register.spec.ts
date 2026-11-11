import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '@core/auth/auth.service';
import { Permissions } from '@core/auth/permissions';
import { AppError } from '@core/http/app-error';
import { ToastService } from '@core/ui/toast.service';
import { of, throwError } from 'rxjs';
import { RegisterApi } from './data/register-api';
import { Register } from './register';

describe('Register', () => {
  const api = { register: vi.fn(), roles: vi.fn() };
  const permissions = signal<Record<string, number>>({});

  async function render() {
    TestBed.configureTestingModule({
      providers: [
        { provide: RegisterApi, useValue: api },
        { provide: AuthService, useValue: { permissions, me: signal({ id: 'admin' }) } },
        { provide: ToastService, useValue: { success: vi.fn(), error: vi.fn() } },
      ],
    });
    const fixture = TestBed.createComponent(Register);
    await fixture.whenStable();
    return fixture;
  }

  function fill(el: HTMLElement, values: Record<string, string>) {
    for (const [id, value] of Object.entries(values)) {
      const input = el.querySelector<HTMLInputElement>(`#${id}`);
      if (input) {
        input.value = value;
        input.dispatchEvent(new Event('input'));
      }
    }
  }

  const submit = (el: HTMLElement) => el.querySelector<HTMLFormElement>('form')?.dispatchEvent(new Event('submit'));

  beforeEach(() => {
    api.register.mockReset();
    api.roles.mockReset().mockReturnValue(of([{ id: 'r1', name: 'Project Manager', description: null, isSystem: false, grants: [] }]));
    permissions.set({ [Permissions.Users.Manage]: 8, [Permissions.Roles.View]: 8 });
  });

  it('does not submit a weak password or mismatched confirmation', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    fill(el, { 'reg-name': 'Rina Das', 'reg-email': 'rina@company.com', 'reg-password': 'weakpass', 'reg-confirm': 'other' });

    submit(el);
    await fixture.whenStable();

    expect(api.register).not.toHaveBeenCalled();
    expect(el.textContent).toContain('doesn’t meet the rules');
    expect(el.textContent).toContain('passwords don’t match');
  });

  it('registers with the chosen role and resets for the next person', async () => {
    api.register.mockReturnValue(of({ id: 'u1', fullName: 'Rina Das', email: 'rina@company.com', roles: ['Project Manager'] }));
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    fill(el, { 'reg-name': ' Rina Das ', 'reg-email': 'rina@company.com', 'reg-password': 'Secret123', 'reg-confirm': 'Secret123' });
    const role = el.querySelector<HTMLSelectElement>('#reg-role');
    if (role) {
      role.value = 'r1';
      role.dispatchEvent(new Event('change'));
    }

    submit(el);
    await fixture.whenStable();

    expect(api.register).toHaveBeenCalledWith({ fullName: 'Rina Das', email: 'rina@company.com', password: 'Secret123', roleId: 'r1' });
    expect(el.textContent).toContain('can now sign in as Project Manager');
    expect(el.querySelector<HTMLInputElement>('#reg-email')?.value).toBe('');
  });

  it('shows a taken email in place and uses the default role without roles.view', async () => {
    permissions.set({ [Permissions.Users.Manage]: 8 });
    api.register.mockReturnValue(throwError(() => new AppError(409, 'A user with this email already exists.')));
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;
    fill(el, { 'reg-name': 'Rina', 'reg-email': 'taken@company.com', 'reg-password': 'Secret123', 'reg-confirm': 'Secret123' });

    submit(el);
    await fixture.whenStable();

    expect(el.querySelector('#reg-role')).toBeNull();
    expect(api.register).toHaveBeenCalledWith(expect.objectContaining({ roleId: null }));
    expect(el.textContent).toContain('A user with this email already exists.');
  });
});
