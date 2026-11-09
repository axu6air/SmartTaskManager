import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { HasPermissionDirective } from '@core/auth/has-permission.directive';
import { Permissions } from '@core/auth/permissions';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskView, TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { TASK_STATUSES, TaskStatus } from '@models/task';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { updateQueryParams } from '@shared/utils/query-params';
import { compareTasks, matchesQuery } from '@shared/utils/task-sort';
import { debounceTime, distinctUntilChanged, skip } from 'rxjs';
import { TaskCard } from './task-card';

@Component({
  selector: 'app-board',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CdkDropListGroup, CdkDropList, CdkDrag, TaskCard, LoadError, Icon, HasPermissionDirective],
  templateUrl: './board.html',
  styleUrl: './board.scss',
})
export class Board {
  protected readonly store = inject(WorkspaceStore);
  protected readonly actions = inject(TaskActions);
  protected readonly views = inject(TaskViews);
  private readonly router = inject(Router);
  protected readonly perms = Permissions;

  /** Query params, bound by the router. */
  readonly project = input<string>();
  readonly mine = input<string>();
  readonly q = input<string>();

  protected readonly search = linkedSignal(() => this.q() ?? '');
  protected readonly projectFilter = computed(() => this.project() ?? '');
  protected readonly mineOnly = computed(() => this.mine() === 'true');

  protected readonly columns = computed(() => {
    const project = this.projectFilter();
    const mineOnly = this.mineOnly();
    const query = this.search();
    const visible = this.views
      .all()
      .filter((v) => (!project || v.task.projectId === project) && (!mineOnly || v.isMine) && matchesQuery(v.task.title, query))
      .sort(compareTasks);

    return TASK_STATUSES.map((status) => ({
      ...status,
      tasks: visible.filter((v) => v.task.status === status.id),
    }));
  });

  constructor() {
    toObservable(this.search)
      .pipe(skip(1), debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => updateQueryParams(this.router, { q }));
  }

  protected setProject(id: string): void {
    updateQueryParams(this.router, { project: id || null });
  }

  protected toggleMine(): void {
    updateQueryParams(this.router, { mine: !this.mineOnly() });
  }

  protected onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected drop(event: CdkDragDrop<TaskStatus, TaskStatus, TaskView>): void {
    if (event.previousContainer !== event.container) {
      void this.actions.changeStatus(event.item.data.task, event.container.data);
    }
  }
}
