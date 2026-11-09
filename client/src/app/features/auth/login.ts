import { ChangeDetectionStrategy, Component, inject, input, isDevMode, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { toAppError } from '@core/http/app-error';
import { Icon } from '@shared/ui/icon';
import { applyServerErrors } from '@shared/utils/server-errors';

interface DemoAccount {
  email: string;
  name: string;
  role: string;
}

// Seeded by the backend's DemoDataSeeder in Development only; never shown in production builds.
const DEMO_PASSWORD = 'password123';
const DEMO_ACCOUNTS: DemoAccount[] = [
  { email: 'admin@email.com', name: 'Admin', role: 'Admin' },
  { email: 'manager1@email.com', name: 'Manager 1', role: 'Project Manager' },
  { email: 'member11@email.com', name: 'Member 11', role: 'Team Member' },
  { email: 'member21@email.com', name: 'Member 21', role: 'Team Member' },
];

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, Icon],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Bound from ?returnUrl= by the router. */
  readonly returnUrl = input<string>();

  protected readonly demoAccounts = isDevMode() ? DEMO_ACCOUNTS : [];
  protected readonly demoPassword = DEMO_PASSWORD;
  protected readonly showPassword = signal(false);
  protected readonly pending = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.maxLength(128)]],
  });

  protected fillDemo(account: DemoAccount): void {
    this.form.setValue({ email: account.email, password: DEMO_PASSWORD });
    this.formError.set(null);
  }

  protected async submit(): Promise<void> {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.pending.set(true);
    const { email, password } = this.form.getRawValue();
    try {
      await this.auth.login(email.trim(), password);
      await this.router.navigateByUrl(this.safeReturnUrl());
    } catch (error) {
      const appError = toAppError(error);
      if (appError.status === 400) {
        const unmatched = applyServerErrors(this.form, appError.fieldErrors);
        this.formError.set(unmatched[0] ?? null);
      } else if (appError.status === 401) {
        this.form.controls.password.reset();
        this.formError.set('Email or password is incorrect. Check them and try again.');
      } else {
        this.formError.set(appError.message);
      }
    } finally {
      this.pending.set(false);
    }
  }

  /** Only same-app paths, so a crafted link can’t send the user to another site after sign-in. */
  private safeReturnUrl(): string {
    const url = this.returnUrl();
    return url && url.startsWith('/') && !url.startsWith('//') ? url : '/';
  }
}
