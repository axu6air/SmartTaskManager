import { Dialog } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { HasPermissionDirective } from '@core/auth/has-permission.directive';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { ToastService } from '@core/ui/toast.service';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskApi } from '@core/workspace/task-api';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { SortDirection } from '@models/api';
import { ProjectSummary } from '@models/project';
import { TASK_PRIORITIES, TASK_STATUSES, TaskPriority, TaskSortField, TaskStatus } from '@models/task';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { Pagination } from '@shared/ui/pagination';
import { pagedResource } from '@shared/utils/paged-resource';
import { pageFrom, updateQueryParams } from '@shared/utils/query-params';
import { debounceTime, distinctUntilChanged, skip } from 'rxjs';
import { openMembersDialog } from '../components/members-dialog';
import { openProjectDialog } from '../components/project-form-dialog';
import { TaskTable } from '../components/task-table';
import { projectStats } from '../models/project-stats';

const PAGE_SIZE = 10;
const SORT_FIELDS: readonly TaskSortField[] = ['title', 'dueDate', 'priority', 'status', 'createdAt'];
/** The direction a column sorts in when first clicked: most urgent first for priority. */
const FIRST_DIRECTION: Record<TaskSortField, SortDirection> = {
  title: 'asc',
  dueDate: 'asc',
  priority: 'desc',
  status: 'asc',
  createdAt: 'desc',
};

@Component({
  selector: 'app-project-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TaskTable, LoadError, Icon, HasPermissionDirective, Pagination],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
})
export class ProjectDetail {
  protected readonly store = inject(WorkspaceStore);
  protected readonly actions = inject(TaskActions);
  private readonly views = inject(TaskViews);
  private readonly taskApi = inject(TaskApi);
  private readonly router = inject(Router);
  private readonly dialog = inject(Dialog);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);
  protected readonly perms = Permissions;
  protected readonly priorities = TASK_PRIORITIES;
  protected readonly pageSize = PAGE_SIZE;

  /** Route param and query params (?status=&priority=&mine=&q=&sort=&dir=&page=), bound by the router. */
  readonly projectId = input.required<string>();
  readonly status = input<string>();
  readonly priority = input<string>();
  readonly mine = input<string>();
  readonly q = input<string>();
  readonly sort = input<string>();
  readonly dir = input<string>();
  readonly page = input<string>();

  /** Follows ?q= (back/forward) but updates instantly while typing; the URL and request catch up after 300ms. */
  protected readonly search = linkedSignal(() => this.q() ?? '');

  protected readonly project = computed(() => this.store.projectById().get(this.projectId()) ?? null);
  /** Every task of the project, for the stat cards and chip counts (the table itself is server-paged). */
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

  protected readonly statusFilter = computed(() => {
    const status = Number(this.status());
    return TASK_STATUSES.some((s) => s.id === status) ? (status as TaskStatus) : 0;
  });
  protected readonly priorityFilter = computed(() => {
    const priority = Number(this.priority());
    return TASK_PRIORITIES.some((p) => p.id === priority) ? (priority as TaskPriority) : 0;
  });
  protected readonly mineOnly = computed(() => this.mine() === 'true');
  protected readonly sortBy = computed<TaskSortField>(() =>
    SORT_FIELDS.includes(this.sort() as TaskSortField) ? (this.sort() as TaskSortField) : 'dueDate',
  );
  protected readonly sortDirection = computed<SortDirection>(() =>
    this.dir() === 'asc' || this.dir() === 'desc' ? this.dir() as SortDirection : FIRST_DIRECTION[this.sortBy()],
  );

  protected readonly chips = computed(() => {
    const tasks = this.projectTasks();
    return [
      { id: 0, label: 'All', count: tasks.length },
      ...TASK_STATUSES.map((s) => ({ ...s, count: tasks.filter((v) => v.task.status === s.id).length })),
    ];
  });

  protected readonly list = pagedResource(
    () => {
      // Read so any task change (created, edited, moved, deleted) refetches the visible page.
      this.store.tasks();
      return {
        projectId: this.projectId(),
        page: pageFrom(this.page()),
        pageSize: PAGE_SIZE,
        search: this.q()?.trim(),
        status: this.statusFilter() || undefined,
        priority: this.priorityFilter() || undefined,
        assigneeId: this.mineOnly() ? this.auth.me()?.id : undefined,
        sortBy: this.sortBy(),
        sortDirection: this.sortDirection(),
      };
    },
    ({ projectId, ...query }) => this.taskApi.list(projectId, query),
  );

  protected readonly rows = computed(() => this.views.toViews()(this.list.data()?.items ?? []));

  protected readonly emptyMessage = computed(() =>
    this.projectTasks().length === 0 ? 'This project has no tasks yet.' : 'No tasks match these filters.',
  );

  constructor() {
    toObservable(this.search)
      .pipe(skip(1), debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => updateQueryParams(this.router, { q, page: null }));

    // Deleting the last task on the last page would leave an empty page: step back to the new last one.
    effect(() => {
      const data = this.list.data();
      if (data && data.items.length === 0 && data.page > 1 && data.totalCount > 0) {
        updateQueryParams(this.router, { page: Math.ceil(data.totalCount / data.pageSize) });
      }
    });
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
    updateQueryParams(this.router, { status: id || null, page: null });
  }

  protected setPriority(event: Event): void {
    updateQueryParams(this.router, { priority: Number((event.target as HTMLSelectElement).value) || null, page: null });
  }

  protected toggleMine(): void {
    updateQueryParams(this.router, { mine: !this.mineOnly(), page: null });
  }

  /** Clicking the active column flips the direction; another column starts in its natural direction. */
  protected setSort(field: TaskSortField): void {
    const dir =
      field === this.sortBy() ? (this.sortDirection() === 'asc' ? 'desc' : 'asc') : FIRST_DIRECTION[field];
    updateQueryParams(this.router, { sort: field, dir, page: null });
  }

  protected setPage(page: number): void {
    updateQueryParams(this.router, { page: page > 1 ? page : null });
  }

  protected onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }
}
