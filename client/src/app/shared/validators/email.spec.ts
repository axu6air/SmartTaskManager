import { FormControl } from '@angular/forms';
import { emailValidator } from './email';

describe('emailValidator', () => {
  const check = (value: string) => emailValidator(new FormControl(value, { nonNullable: true }));

  it.each(['name@company.com', 'first.last+tag@mail.company.co.uk', 'MEMBER11@EMAIL.COM', '  name@company.com  '])(
    'accepts %s',
    (email) => expect(check(email)).toBeNull(),
  );

  it.each(['name@company', 'name@company.c', 'name@@company.com', 'name company@x.com', '@company.com', 'name@.com', 'plainaddress'])(
    'rejects %s',
    (email) => expect(check(email)).toEqual({ email: true }),
  );

  it('leaves an empty value to the required validator', () => {
    expect(check('')).toBeNull();
  });
});
