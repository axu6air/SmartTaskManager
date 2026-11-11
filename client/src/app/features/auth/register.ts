import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { toAppError } from '@core/http/app-error';
import { ToastService } from '@core/ui/toast.service';
import { RegisteredUser } from '@models/auth';
import { Icon } from '@shared/ui/icon';
import { emailValidator } from '@shared/validators/email';
import { applyServerErrors } from '@shared/utils/server-errors';
import { firstValueFrom, of } from 'rxjs';
import { RegisterApi } from './data/register-api';

/** Same policy as the server's RegisterUserCommandValidator. */
const PASSWORD_RULES = [Validators.required, Validators.minLength(8), Validators.maxLength(128), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$/)];

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const { password, confirmPassword } = group.value as { password: string; confirmPassword: string };
  return confirmPassword && password !== confirmPassword ? { mismatch: true } : null;
}

/** Admins (Manage Users) create accounts for other people; there is no self-registration. */
@Component({
  selector: 'app-register',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, Icon],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly api = inject(RegisterApi);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);

  protected readonly pending = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly showPassword = signal(false);
  protected readonly created = signal<RegisteredUser | null>(null);

  /** Without roles.view the role list can't be read, so the server's default role is used. */
  protected readonly canChooseRole = computed(() => {
    this.auth.permissions();
    return this.permissions.has(Permissions.Roles.View);
  });
  private readonly rolesResource = rxResource({
    params: () => this.canChooseRole(),
    stream: ({ params }) => (params ? this.api.roles() : of([])),
  });
  protected readonly roles = computed(() => (this.rolesResource.hasValue() ? this.rolesResource.value() : []));

  protected readonly form = inject(NonNullableFormBuilder).group(
    {
      fullName: ['', [Validators.required, Validators.maxLength(150)]],
      email: ['', [Validators.required, emailValidator, Validators.maxLength(256)]],
      password: ['', PASSWORD_RULES],
      confirmPassword: ['', [Validators.required]],
      roleId: [''],
    },
    { validators: passwordsMatch },
  );

  /** Used by the route's canDeactivate guard. */
  hasUnsavedChanges(): boolean {
    return this.form.dirty && !this.pending();
  }

  protected async submit(): Promise<void> {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.pending.set(true);
    try {
      const user = await firstValueFrom(
        this.api.register({
          fullName: value.fullName.trim(),
          email: value.email.trim(),
          password: value.password,
          roleId: value.roleId || null,
        }),
      );
      this.created.set(user);
      this.toast.success(`Account created for ${user.fullName}`);
      // Ready for the next person; keeps the chosen role, which admins often reuse.
      this.form.reset({ fullName: '', email: '', password: '', confirmPassword: '', roleId: value.roleId });
    } catch (error) {
      const appError = toAppError(error);
      if (appError.status === 400) {
        const unmatched = applyServerErrors(this.form, appError.fieldErrors);
        this.formError.set(unmatched[0] ?? (Object.keys(appError.fieldErrors).length ? null : appError.message));
      } else if (appError.status === 409 || appError.status === 404) {
        // Conflict (email taken, no default role) and a deleted role are fixable here, so show them in place.
        this.formError.set(appError.message);
      }
    } finally {
      this.pending.set(false);
    }
  }
}
