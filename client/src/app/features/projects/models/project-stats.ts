import { TaskView } from '@core/workspace/task-views';
import { TaskStatus } from '@models/task';

export interface ProjectStats {
  total: number;
  open: number;
  done: number;
  overdue: number;
  mine: number;
  /** Tasks that weren't cancelled: the base for progress. */
  countable: number;
  /** Completed share of countable tasks, 0–100. */
  percentDone: number;
}

export function projectStats(views: readonly TaskView[]): ProjectStats {
  const done = views.filter((v) => v.task.status === TaskStatus.Completed).length;
  const cancelled = views.filter((v) => v.task.status === TaskStatus.Cancelled).length;
  const countable = views.length - cancelled;

  return {
    total: views.length,
    open: views.filter((v) => !v.closed).length,
    done,
    overdue: views.filter((v) => v.due.tone === 'overdue').length,
    mine: views.filter((v) => v.isMine && !v.closed).length,
    countable,
    percentDone: countable > 0 ? Math.round((done / countable) * 100) : 0,
  };
}
