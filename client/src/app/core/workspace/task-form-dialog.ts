import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AppError, toAppError } from '@core/http/app-error';
import { Guid } from '@models/api';
import { ProjectSummary } from '@models/project';
import { ProjectTask, TASK_PRIORITIES, TaskPriority } from '@models/task';
import { Icon } from '@shared/ui/icon';
import { endOfLocalDayIso, isoToLocalYmd, todayYmd } from '@shared/utils/date';
import { applyServerErrors } from '@shared/utils/server-errors';
import { WorkspaceStore } from './workspace.store';

export interface TaskFormData {
  /** Absent when creating. */
  task?: ProjectTask;
  /** Projects offered when creating. */
  projects: ProjectSummary[];
  defaultProjectId?: Guid;
  /** Editing only: whether the assignee may be changed. */
  canAssign: boolean;
}

function notInPast(control: AbstractControl<string>): ValidationErrors | null {
  return control.value && control.value < todayYmd() ? { past: true } : null;
}

@Component({
  selector: 'app-task-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, Icon],
  templateUrl: './task-form-dialog.html',
})
export class TaskFormDialog {
  private readonly store = inject(WorkspaceStore);
  private readonly ref = inject<DialogRef<boolean>>(DialogRef);
  protected readonly data = inject<TaskFormData>(DIALOG_DATA);
  protected readonly isEdit = this.data.task !== undefined;
  protected readonly priorities = TASK_PRIORITIES;
  protected readonly today = todayYmd();

  protected readonly pending = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly confirmingDiscard = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group({
    title: [this.data.task?.title ?? '', [Validators.required, Validators.maxLength(200)]],
    description: [this.data.task?.description ?? '', [Validators.maxLength(4000)]],
    projectId: [this.data.task?.projectId ?? this.data.defaultProjectId ?? this.data.projects[0]?.id ?? '', [Validators.required]],
    assigneeId: [this.data.task?.assigneeId ?? ''],
    priority: [this.data.task?.priority ?? (TaskPriority.Medium as TaskPriority)],
    // The server only forbids past dates on creation; an existing task may legitimately be overdue.
    dueDate: [
      this.data.task?.dueDate ? isoToLocalYmd(this.data.task.dueDate) : '',
      this.isEdit ? [] : [notInPast],
    ],
  });

  private readonly projectId = toSignal(this.form.controls.projectId.valueChanges, {
    initialValue: this.form.controls.projectId.value,
  });

  protected readonly members = computed(() => this.store.members()[this.projectId()] ?? []);
  protected readonly showAssignee = !this.isEdit || this.data.canAssign;

  constructor() {
    this.ref.disableClose = true;
    this.ref.backdropClick.pipe(takeUntilDestroyed()).subscribe(() => this.requestClose());
    this.ref.keydownEvents.pipe(takeUntilDestroyed()).subscribe((event) => {
      if (event.key === 'Escape') {
        this.requestClose();
      }
    });
    // A new project has different members, so a previous assignee may no longer be valid.
    this.form.controls.projectId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.assigneeId.setValue(''));
  }

  protected requestClose(): void {
    if (this.form.dirty && !this.pending()) {
      this.confirmingDiscard.set(true);
    } else {
      this.ref.close(false);
    }
  }

  protected discard(): void {
    this.ref.close(false);
  }

  protected setPriority(priority: TaskPriority): void {
    this.form.controls.priority.setValue(priority);
    this.form.markAsDirty();
  }

  protected async submit(): Promise<void> {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      title: value.title.trim(),
      description: value.description.trim() || null,
      priority: value.priority,
      dueDate: value.dueDate ? endOfLocalDayIso(value.dueDate) : null,
    };
    const assigneeId = value.assigneeId || null;

    this.pending.set(true);
    try {
      if (this.data.task) {
        await this.store.updateTask(this.data.task, body, this.data.canAssign ? assigneeId : undefined);
      } else {
        await this.store.createTask(value.projectId, { ...body, assigneeId });
      }
      this.ref.close(true);
    } catch (error) {
      this.showError(toAppError(error));
    } finally {
      this.pending.set(false);
    }
  }

  private showError(error: AppError): void {
    if (error.status === 400) {
      const unmatched = applyServerErrors(this.form, error.fieldErrors);
      this.formError.set(unmatched[0] ?? (Object.keys(error.fieldErrors).length ? null : error.message));
    }
  }
}
