import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Full-page message for unknown URLs and for users whose roles open no page at all. */
@Component({
  selector: 'app-status-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <section class="card state">
      <h1 class="code">{{ code() }}</h1>
      <h2>{{ heading() }}</h2>
      <p>{{ message() }}</p>
      @if (showHomeLink()) {
        <a class="btn btn-primary" routerLink="/">Go to start page</a>
      }
    </section>
  `,
  styles: `
    .code {
      font-family: var(--font-display);
      font-size: 48px;
      color: var(--accent);
    }
  `,
})
export class StatusPage {
  readonly code = input('404');
  readonly heading = input('Page not found');
  readonly message = input('The page you’re looking for doesn’t exist or was moved.');
  readonly showHomeLink = input(true);
}
