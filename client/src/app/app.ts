import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from '@core/ui/theme.service';
import { ToastHost } from '@core/ui/toast-host';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, ToastHost],
  template: `<router-outlet /><app-toast-host />`,
})
export class App {
  constructor() {
    // Created at start-up so the saved theme applies before the first page renders.
    inject(ThemeService);
  }
}
