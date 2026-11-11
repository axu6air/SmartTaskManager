import { AbstractControl, ValidationErrors } from '@angular/forms';

/**
 * local@domain.tld: no spaces, exactly one @, a dot in the domain, an ending of 2+ letters.
 * Same pattern as the server's EmailRule, so the form never accepts what the API rejects.
 * Stricter than Validators.email, which accepts "name@company".
 */
export const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/;

/** Reports `{ email: true }`, the same key as Validators.email; empty values are left to `required`. */
export function emailValidator(control: AbstractControl<string>): ValidationErrors | null {
  const value = (control.value ?? '').trim();
  return value && !EMAIL_PATTERN.test(value) ? { email: true } : null;
}
