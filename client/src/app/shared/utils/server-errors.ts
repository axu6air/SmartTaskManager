import { AbstractControl, FormGroup } from '@angular/forms';

/**
 * Puts server validation messages on the matching controls as a `server` error.
 * Server keys are C# property names ("DueDate"); controls are camelCase ("dueDate").
 * Returns the messages that matched no control, for a form-level alert.
 */
export function applyServerErrors(
  form: FormGroup,
  errors: Record<string, string[]>,
): string[] {
  const controls = new Map<string, AbstractControl>(
    Object.entries(form.controls).map(([name, control]) => [name.toLowerCase(), control]),
  );
  const unmatched: string[] = [];

  for (const [key, messages] of Object.entries(errors)) {
    const control = controls.get(key.toLowerCase());
    if (control && messages.length > 0) {
      control.setErrors({ ...control.errors, server: messages[0] });
      control.markAsTouched();
    } else {
      unmatched.push(...messages);
    }
  }

  return unmatched;
}
