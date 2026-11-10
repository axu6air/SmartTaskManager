import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TaskView } from '@core/workspace/task-views';
import { Guid, SortDirection } from '@models/api';
import { PRIORITY_LABEL, STATUS_LABEL, TaskSortField } from '@models/task';
import { Avatar } from '@shared/ui/avatar';

const COLUMNS: { label: string; field: TaskSortField | null }[] = [
  { label: 'Task', field: 'title' },
  { label: 'Status', field: 'status' },
  { label: 'Priority', field: 'priority' },
  { label: 'Assignee', field: null },
  { label: 'Due', field: 'dueDate' },
];

@Component({
  selector: 'app-task-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Avatar],
  templateUrl: './task-table.html',
  styleUrl: './task-table.scss',
})
export class TaskTable {
  readonly tasks = input.required<TaskView[]>();
  readonly emptyMessage = input('No tasks match these filters.');
  readonly sortBy = input<TaskSortField>('dueDate');
  readonly sortDirection = input<SortDirection>('asc');
  /** True while the next page or sort is loading; the current rows stay visible. */
  readonly loading = input(false);
  readonly open = output<Guid>();
  readonly sortChange = output<TaskSortField>();

  protected readonly columns = COLUMNS;
  protected readonly statusLabel = STATUS_LABEL;
  protected readonly priorityLabel = PRIORITY_LABEL;
}
