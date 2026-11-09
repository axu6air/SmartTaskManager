import { TaskStatus } from '@models/task';

interface Sortable {
  closed: boolean;
  task: { dueDate: string | null; status: TaskStatus; title: string };
}

const STATUS_ORDER: Record<TaskStatus, number> = { 2: 0, 1: 1, 3: 2, 4: 3 };

/** Open before closed, then earliest due (no date last), then in-progress first, then title. */
export function compareTasks(a: Sortable, b: Sortable): number {
  if (a.closed !== b.closed) {
    return a.closed ? 1 : -1;
  }
  const aDue = a.task.dueDate ? Date.parse(a.task.dueDate) : Number.POSITIVE_INFINITY;
  const bDue = b.task.dueDate ? Date.parse(b.task.dueDate) : Number.POSITIVE_INFINITY;
  if (aDue !== bDue) {
    return aDue - bDue;
  }
  const byStatus = STATUS_ORDER[a.task.status] - STATUS_ORDER[b.task.status];
  return byStatus !== 0 ? byStatus : a.task.title.localeCompare(b.task.title);
}

/** Case-insensitive title match; an empty query matches everything. */
export function matchesQuery(title: string, query: string): boolean {
  const q = query.trim().toLowerCase();
  return q === '' || title.toLowerCase().includes(q);
}
