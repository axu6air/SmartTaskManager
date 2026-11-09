import { A11yModule } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { LoadingService } from '@core/http/loading.service';
import { ThemeService } from '@core/ui/theme.service';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { Avatar } from '@shared/ui/avatar';
import { Icon, IconName } from '@shared/ui/icon';
import { filter } from 'rxjs';
import { NAV_ITEMS } from './navigation';

const THEME_ICON: Record<string, IconName> = { system: 'monitor', light: 'sun', dark: 'moon' };
const THEME_LABEL: Record<string, string> = { system: 'System theme', light: 'Light theme', dark: 'Dark theme' };

@Component({
  selector: 'app-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, A11yModule, Avatar, Icon],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
  host: { '(document:keydown.escape)': 'menuOpen.set(false)' },
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);
  private readonly store = inject(WorkspaceStore);
  private readonly views = inject(TaskViews);
  private readonly theme = inject(ThemeService);
  protected readonly loading = inject(LoadingService);

  protected readonly me = this.auth.me;
  protected readonly menuOpen = signal(false);
  protected readonly signingOut = signal(false);

  protected readonly nav = computed(() => {
    this.auth.permissions();
    const badges = {
      projects: this.store.projects().length,
      myOpenTasks: this.views.all().filter((v) => v.isMine && !v.closed).length,
    };
    return NAV_ITEMS.filter((item) => this.permissions.hasAny(item.permissions)).map((item) => ({
      ...item,
      count: item.badge ? badges[item.badge] : 0,
    }));
  });

  protected readonly roleLabel = computed(() => this.me()?.roles.join(', ') || 'No role');
  protected readonly themeIcon = computed(() => THEME_ICON[this.theme.choice()]);
  protected readonly themeLabel = computed(() => THEME_LABEL[this.theme.choice()]);

  constructor() {
    if (this.store.status() === 'idle') {
      inject(TaskActions).reloadWorkspace();
    }
    inject(Router)
      .events.pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.menuOpen.set(false));
  }

  protected cycleTheme(): void {
    this.theme.cycle();
  }

  protected async signOut(): Promise<void> {
    this.signingOut.set(true);
    await this.auth.logout();
  }
}
