import { Dialog } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { ToastService } from '@core/ui/toast.service';
import { ProjectSummary } from '@models/project';
import { openMembersDialog } from '../components/members-dialog';
import { openProjectDialog } from '../components/project-form-dialog';
import { toObservable, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { HasPermissionDirective } from '@core/auth/has-permission.directive';
import { Permissions } from '@core/auth/permissions';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { TASK_STATUSES } from '@models/task';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { updateQueryParams } from '@shared/utils/query-params';
import { compareTasks, matchesQuery } from '@shared/utils/task-sort';
import { debounceTime, distinctUntilChanged, skip } from 'rxjs';
import { TaskTable } from '../components/task-table';
import { projectStats } from '../models/project-stats';

@Component({
  selector: 'app-project-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TaskTable, LoadError, Icon, HasPermissionDirective],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
})
export class ProjectDetail {
  protected readonly store = inject(WorkspaceStore);
  protected readonly actions = inject(TaskActions);
  private readonly views = inject(TaskViews);
  private readonly router = inject(Router);
  private readonly dialog = inject(Dialog);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);
  protected readonly perms = Permissions;

  /** Route param and query params, bound by the router. */
  readonly projectId = input.required<string>();
  readonly status = input<string>();
  readonly mine = input<string>();
  readonly q = input<string>();

  /** Follows ?q= (back/forward) but updates instantly while typing; the URL catches up after 300ms. */
  protected readonly search = linkedSignal(() => this.q() ?? '');

  protected readonly project = computed(() => this.store.projectById().get(this.projectId()) ?? null);
  private readonly projectTasks = computed(() => this.views.forProject(this.projectId()));
  protected readonly stats = computed(() => projectStats(this.projectTasks()));
  protected readonly canCreate = computed(() =>
    this.views.creatableProjects().some((p) => p.id === this.projectId()),
  );

  /** Scope-aware: a manager may edit only projects they manage. */
  protected readonly canEdit = computed(() => {
    this.auth.permissions();
    return this.permissions.canOnProject(Permissions.Projects.Update, this.store.members()[this.projectId()] ?? []);
  });

  protected readonly canManageMembers = computed(() => {
    this.auth.permissions();
    return this.permissions.canOnProject(Permissions.Projects.ManageMembers, this.store.members()[this.projectId()] ?? []);
  });

  protected readonly statusFilter = computed(() => Number(this.status()) || 0);
  protected readonly mineOnly = computed(() => this.mine() === 'true');

  protected readonly chips = computed(() => {
    const tasks = this.projectTasks();
    return [
      { id: 0, label: 'All', count: tasks.length },
      ...TASK_STATUSES.map((s) => ({ ...s, count: tasks.filter((v) => v.task.status === s.id).length })),
    ];
  });

  protected readonly rows = computed(() => {
    const status = this.statusFilter();
    const mineOnly = this.mineOnly();
    const query = this.search();
    return this.projectTasks()
      .filter((v) => (status === 0 || v.task.status === status) && (!mineOnly || v.isMine) && matchesQuery(v.task.title, query))
      .sort(compareTasks);
  });

  protected readonly emptyMessage = computed(() =>
    this.projectTasks().length === 0 ? 'This project has no tasks yet.' : 'No tasks match these filters.',
  );

  constructor() {
    toObservable(this.search)
      .pipe(skip(1), debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => updateQueryParams(this.router, { q }));
  }

  protected edit(project: ProjectSummary): void {
    openProjectDialog(this.dialog, project).subscribe((saved) => {
      if (saved) {
        this.toast.success('Project updated');
      }
    });
  }

  protected manageMembers(project: ProjectSummary): void {
    openMembersDialog(this.dialog, project);
  }

  protected setStatus(id: number): void {
    updateQueryParams(this.router, { status: id || null });
  }

  protected toggleMine(): void {
    updateQueryParams(this.router, { mine: !this.mineOnly() });
  }

  protected onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }
}
