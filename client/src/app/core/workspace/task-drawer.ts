import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { Guid } from '@models/api';
import { PRIORITY_LABEL, TASK_STATUSES, TaskStatus } from '@models/task';
import { OffsetDatePipe } from '@shared/pipes/offset-date.pipe';
import { Avatar } from '@shared/ui/avatar';
import { Icon } from '@shared/ui/icon';
import { TaskActions } from './task-actions';
import { TaskViews } from './task-views';

/** Side panel with one task's details and the actions this user may take on it. */
@Component({
  selector: 'app-task-drawer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Avatar, Icon, OffsetDatePipe],
  templateUrl: './task-drawer.html',
  styleUrl: './task-drawer.scss',
})
export class TaskDrawer {
  private readonly ref = inject(DialogRef);
  private readonly taskId = inject<Guid>(DIALOG_DATA);
  private readonly views = inject(TaskViews);
  private readonly actions = inject(TaskActions);

  protected readonly statuses = TASK_STATUSES;
  protected readonly priorityLabel = PRIORITY_LABEL;
  protected readonly view = computed(() => this.views.byId().get(this.taskId) ?? null);

  constructor() {
    // The task disappeared (deleted here or elsewhere, or the user signed out): nothing left to show.
    effect(() => {
      if (this.view() === null) {
        this.ref.close();
      }
    });
  }

  protected close(): void {
    this.ref.close();
  }

  protected setStatus(status: TaskStatus): void {
    const view = this.view();
    if (view) {
      void this.actions.changeStatus(view.task, status);
    }
  }

  protected edit(): void {
    const view = this.view();
    if (view) {
      this.actions.openEdit(view);
    }
  }

  protected async remove(): Promise<void> {
    const view = this.view();
    if (view && (await this.actions.delete(view.task))) {
      this.ref.close();
    }
  }
}
