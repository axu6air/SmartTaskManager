import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TaskView } from '@core/workspace/task-views';
import { Guid } from '@models/api';
import { PRIORITY_LABEL, STATUS_LABEL } from '@models/task';
import { Avatar } from '@shared/ui/avatar';

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
  readonly open = output<Guid>();

  protected readonly statusLabel = STATUS_LABEL;
  protected readonly priorityLabel = PRIORITY_LABEL;
}
