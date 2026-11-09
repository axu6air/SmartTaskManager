import { Guid, OffsetDateString } from './api';

/** Mirrors ProjectTaskStatus. */
export const TaskStatus = {
  ToDo: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
} as const;
export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus];

/** Mirrors ProjectTaskPriority. */
export const TaskPriority = {
  Low: 1,
  Medium: 2,
  High: 3,
  Critical: 4,
} as const;
export type TaskPriority = (typeof TaskPriority)[keyof typeof TaskPriority];

export const TASK_STATUSES: readonly { id: TaskStatus; label: string }[] = [
  { id: TaskStatus.ToDo, label: 'To do' },
  { id: TaskStatus.InProgress, label: 'In progress' },
  { id: TaskStatus.Completed, label: 'Completed' },
  { id: TaskStatus.Cancelled, label: 'Cancelled' },
];

export const TASK_PRIORITIES: readonly { id: TaskPriority; label: string }[] = [
  { id: TaskPriority.Low, label: 'Low' },
  { id: TaskPriority.Medium, label: 'Medium' },
  { id: TaskPriority.High, label: 'High' },
  { id: TaskPriority.Critical, label: 'Critical' },
];

export const STATUS_LABEL: Record<TaskStatus, string> = {
  1: 'To do',
  2: 'In progress',
  3: 'Completed',
  4: 'Cancelled',
};

export const PRIORITY_LABEL: Record<TaskPriority, string> = {
  1: 'Low',
  2: 'Medium',
  3: 'High',
  4: 'Critical',
};

/** Completed and cancelled tasks are closed: they never count as open or overdue. */
export function isClosed(status: TaskStatus): boolean {
  return status === TaskStatus.Completed || status === TaskStatus.Cancelled;
}

/** Mirrors ProjectTaskResponse. */
export interface ProjectTask {
  id: Guid;
  projectId: Guid;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: OffsetDateString | null;
  assigneeId: Guid | null;
  completedAt: OffsetDateString | null;
}

/** Mirrors CreateTaskRequest. */
export interface CreateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: OffsetDateString | null;
  assigneeId: Guid | null;
}

/** Mirrors UpdateTaskRequest. */
export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: OffsetDateString | null;
}
