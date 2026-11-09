import { FormControl, FormGroup } from '@angular/forms';
import { applyServerErrors } from './server-errors';

describe('applyServerErrors', () => {
  it('maps PascalCase server keys onto camelCase controls and returns the rest', () => {
    const form = new FormGroup({ title: new FormControl(''), dueDate: new FormControl('') });

    const unmatched = applyServerErrors(form, {
      Title: ['Title is required.'],
      DueDate: ['The due date cannot be in the past.'],
      AssigneeId: ['Not a member.'],
    });

    expect(form.controls.title.errors).toEqual({ server: 'Title is required.' });
    expect(form.controls.dueDate.errors?.['server']).toBe('The due date cannot be in the past.');
    expect(form.controls.title.touched).toBe(true);
    expect(unmatched).toEqual(['Not a member.']);
  });
});
