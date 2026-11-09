import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { HasPermissionDirective } from './has-permission.directive';
import { Permissions } from './permissions';

const permissions = signal<Record<string, number>>({});

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [HasPermissionDirective],
  template: `<button *appHasPermission="code">Delete</button>`,
})
class Host {
  protected readonly code = Permissions.Tasks.Delete;
}

describe('HasPermissionDirective', () => {
  beforeEach(() => {
    permissions.set({});
    TestBed.configureTestingModule({
      providers: [{ provide: AuthService, useValue: { permissions, me: signal({ id: 'me' }) } }],
    });
  });

  it('shows the element only while the permission is held', async () => {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('button')).toBeNull();

    permissions.set({ [Permissions.Tasks.Delete]: 8 });
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('button')).not.toBeNull();

    permissions.set({});
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
  });
});
