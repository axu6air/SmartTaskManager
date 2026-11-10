import { Dialog } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { HasPermissionDirective } from '@core/auth/has-permission.directive';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { ConfirmDialogService } from '@core/ui/confirm-dialog.service';
import { ToastService } from '@core/ui/toast.service';
import { ProjectApi } from '@core/workspace/project-api';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { SortDirection } from '@models/api';
import { ProjectSortField, ProjectSummary } from '@models/project';
import { PermissionScope } from '@models/scope';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { Pagination } from '@shared/ui/pagination';
import { pagedResource } from '@shared/utils/paged-resource';
import { pageFrom, updateQueryParams } from '@shared/utils/query-params';
import { debounceTime, distinctUntilChanged, skip } from 'rxjs';
import { ProjectCard } from '../components/project-card';
import { openProjectDialog } from '../components/project-form-dialog';
import { projectStats } from '../models/project-stats';

const PAGE_SIZE = 12;

const SORT_OPTIONS: { value: string; label: string }[] = [
  { value: 'name:asc', label: 'Name (A–Z)' },
  { value: 'name:desc', label: 'Name (Z–A)' },
  { value: 'createdAt:desc', label: 'Newest first' },
  { value: 'createdAt:asc', label: 'Oldest first' },
  { value: 'startDate:asc', label: 'Start date' },
  { value: 'endDate:asc', label: 'End date' },
];
const SORT_FIELDS: readonly ProjectSortField[] = ['name', 'startDate', 'endDate', 'createdAt'];

@Component({
  selector: 'app-project-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProjectCard, LoadError, Icon, HasPermissionDirective, Pagination],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss',
})
export class ProjectList {
  protected readonly store = inject(WorkspaceStore);
  protected readonly views = inject(TaskViews);
  protected readonly actions = inject(TaskActions);
  private readonly api = inject(ProjectApi);
  private readonly permissions = inject(PermissionService);
  private readonly dialog = inject(Dialog);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly auth = inject(AuthService);
  protected readonly perms = Permissions;
  protected readonly skeletons = [1, 2, 3];
  protected readonly sortOptions = SORT_OPTIONS;
  protected readonly pageSize = PAGE_SIZE;

  /** Query params, bound by the router: ?q=&sort=&dir=&page= */
  readonly q = input<string>();
  readonly sort = input<string>();
  readonly dir = input<string>();
  readonly page = input<string>();

  /** Follows ?q= but updates instantly while typing; the URL (and so the request) catches up after 300ms. */
  protected readonly search = linkedSignal(() => this.q() ?? '');
  private readonly sortBy = computed<ProjectSortField>(() =>
    SORT_FIELDS.includes(this.sort() as ProjectSortField) ? (this.sort() as ProjectSortField) : 'name',
  );
  private readonly sortDirection = computed<SortDirection>(() => (this.dir() === 'desc' ? 'desc' : 'asc'));
  protected readonly sortValue = computed(() => `${this.sortBy()}:${this.sortDirection()}`);
  protected readonly currentPage = computed(() => pageFrom(this.page()));

  protected readonly list = pagedResource(
    () => {
      // Read so creating, editing or deleting a project refetches the current page.
      this.store.projects();
      return {
        page: this.currentPage(),
        pageSize: PAGE_SIZE,
        search: this.q()?.trim(),
        sortBy: this.sortBy(),
        sortDirection: this.sortDirection(),
      };
    },
    (query) => this.api.list(query),
  );

  protected readonly subtitle = computed(() =>
    (this.permissions.scopeOf(Permissions.Projects.View) & PermissionScope.All) !== 0
      ? 'Every project in the workspace. Pick one to see its tasks.'
      : 'Projects you’re a member of. Pick one to see its tasks.',
  );

  protected readonly cards = computed(() => {
    this.auth.permissions();
    const members = this.store.members();
    const tasksByProject = this.store.tasksByProject();
    const toViews = this.views.toViews();
    return (this.list.data()?.items ?? []).map((project) => {
      const projectMembers = members[project.id] ?? [];
      return {
        project,
        members: projectMembers,
        stats: projectStats(toViews(tasksByProject.get(project.id) ?? [])),
        // Scope-aware, like the server: a manager may only edit or delete projects they manage.
        canEdit: this.permissions.canOnProject(Permissions.Projects.Update, projectMembers),
        canDelete: this.permissions.canOnProject(Permissions.Projects.Delete, projectMembers),
      };
    });
  });

  constructor() {
    toObservable(this.search)
      .pipe(skip(1), debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => updateQueryParams(this.router, { q, page: null }));

    // Deleting the last project on the last page would leave an empty page: step back to the new last one.
    effect(() => {
      const data = this.list.data();
      if (data && data.items.length === 0 && data.page > 1 && data.totalCount > 0) {
        updateQueryParams(this.router, { page: Math.ceil(data.totalCount / data.pageSize) });
      }
    });
  }

  protected onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected setSort(event: Event): void {
    const [sort, dir] = (event.target as HTMLSelectElement).value.split(':');
    updateQueryParams(this.router, { sort, dir, page: null });
  }

  protected setPage(page: number): void {
    updateQueryParams(this.router, { page: page > 1 ? page : null });
  }

  protected editProject(project: ProjectSummary): void {
    openProjectDialog(this.dialog, project).subscribe((saved) => {
      if (saved) {
        this.toast.success('Project updated');
      }
    });
  }

  protected async deleteProject(project: ProjectSummary, taskCount: number): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      title: `Delete “${project.name}”?`,
      message:
        taskCount > 0
          ? `The project and its ${taskCount} ${taskCount === 1 ? 'task' : 'tasks'} will be removed for everyone.`
          : 'The project will be removed for everyone.',
      confirmLabel: 'Delete project',
      cancelLabel: 'Keep it',
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.store.deleteProject(project.id);
      this.toast.success('Project deleted');
    } catch {
      // The error interceptor already showed why.
    }
  }

  protected createProject(): void {
    openProjectDialog(this.dialog).subscribe((projectId) => {
      if (projectId) {
        this.toast.success('Project created');
        void this.router.navigate(['/projects', projectId]);
      }
    });
  }
}
