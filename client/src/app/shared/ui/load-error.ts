import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Icon } from './icon';

@Component({
  selector: 'app-load-error',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  template: `
    <section class="card state" role="alert">
      <app-icon name="alert" [size]="28" />
      <h2>We couldn’t load this page</h2>
      <p>{{ message() }}</p>
      <button type="button" class="btn btn-primary" (click)="retry.emit()">Try again</button>
    </section>
  `,
  styles: `
    app-icon {
      color: var(--danger);
    }
  `,
})
export class LoadError {
  readonly message = input('Something went wrong.');
  readonly retry = output();
}
