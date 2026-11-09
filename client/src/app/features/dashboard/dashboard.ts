import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { TASK_STATUSES, TaskStatus } from '@models/task';
import { LoadError } from '@shared/ui/load-error';
import { compareTasks } from '@shared/utils/task-sort';

const UPCOMING_COUNT = 6;

function percent(part: number, whole: number): number {
  return whole > 0 ? Math.round((part / whole) * 100) : 0;
}

@Component({
  selector: 'app-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, LoadError],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  protected readonly store = inject(WorkspaceStore);
  protected readonly actions = inject(TaskActions);
  private readonly views = inject(TaskViews);

  protected readonly summary = computed(() => {
    const all = this.views.all();
    const count = (status: TaskStatus) => all.filter((v) => v.task.status === status).length;
    const completed = count(TaskStatus.Completed);
    return {
      total: all.length,
      inProgress: count(TaskStatus.InProgress),
      toDo: count(TaskStatus.ToDo),
      completed,
      completedPercent: percent(completed, all.length - count(TaskStatus.Cancelled)),
      overdue: all.filter((v) => v.due.tone === 'overdue').length,
      mineOpen: all.filter((v) => v.isMine && !v.closed).length,
    };
  });

  protected readonly byStatus = computed(() => {
    const all = this.views.all();
    return TASK_STATUSES.map((s) => {
      const count = all.filter((v) => v.task.status === s.id).length;
      return { ...s, count, percent: percent(count, all.length) };
    });
  });

  protected readonly upcoming = computed(() =>
    this.views
      .all()
      .filter((v) => !v.closed && v.task.dueDate !== null)
      .sort(compareTasks)
      .slice(0, UPCOMING_COUNT),
  );

  protected readonly projectProgress = computed(() => {
    const byProject = this.store.tasksByProject();
    return this.store.projects().map((p) => {
      const tasks = (byProject.get(p.id) ?? []).filter((t) => t.status !== TaskStatus.Cancelled);
      const done = tasks.filter((t) => t.status === TaskStatus.Completed).length;
      return { id: p.id, name: p.name, done, total: tasks.length, percent: percent(done, tasks.length) };
    });
  });
}
