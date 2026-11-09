import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TaskView } from '@core/workspace/task-views';
import { PRIORITY_LABEL } from '@models/task';
import { Avatar } from '@shared/ui/avatar';

@Component({
  selector: 'app-task-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Avatar],
  template: `
    <button type="button" class="tcard" [class.locked]="!view().canChangeStatus" (click)="open.emit()">
      <span class="spread">
        <span class="pill p-{{ view().task.priority }}">{{ priorityLabel[view().task.priority] }}</span>
        <span class="hint project">{{ view().projectName }}</span>
      </span>
      <span class="t" [class.done]="view().closed">{{ view().task.title }}</span>
      <span class="spread">
        <span class="who small">
          <app-avatar [name]="view().assigneeName" [size]="22" />
          <span>{{ view().assigneeName ?? 'Unassigned' }}</span>
        </span>
        <span class="due" [class.overdue]="view().due.tone === 'overdue'" [class.today]="view().due.tone === 'today'">
          {{ view().due.label }}
        </span>
      </span>
    </button>
  `,
  styles: `
    :host {
      display: block;
    }
    .tcard {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      text-align: left;
      padding: 14px;
      border-radius: 10px;
      background: var(--surface);
      border: 1px solid var(--line);
      box-shadow: var(--shadow);
      cursor: grab;
    }
    .tcard.locked {
      cursor: pointer;
    }
    .tcard:hover {
      border-color: var(--accent);
    }
    :host(.cdk-drag-preview) .tcard {
      box-shadow: var(--shadow-lg);
      cursor: grabbing;
    }
    .t {
      font-weight: 600;
      line-height: 1.35;
      overflow-wrap: anywhere;
    }
    .t.done {
      color: var(--muted);
      text-decoration: line-through;
    }
    .project {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .small {
      font-size: 12px;
    }
  `,
})
export class TaskCard {
  readonly view = input.required<TaskView>();
  readonly open = output();
  protected readonly priorityLabel = PRIORITY_LABEL;
}
