import { DIALOG_DATA, Dialog, DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { toAppError } from '@core/http/app-error';
import { AiSuggest } from '@core/workspace/ai-suggest';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { AiSuggestion } from '@models/task';
import { Guid } from '@models/api';
import { ProjectSummary } from '@models/project';
import { Icon } from '@shared/ui/icon';
import { endOfLocalDayIso, isoToLocalYmd, startOfLocalDayIso } from '@shared/utils/date';
import { Observable } from 'rxjs';
import { applyServerErrors } from '@shared/utils/server-errors';

/** Same rule as the server's ProjectFieldsValidator. Date-input values compare correctly as strings. */
function endNotBeforeStart(group: AbstractControl): ValidationErrors | null {
  const { startDate, endDate } = group.value as { startDate: string; endDate: string };
  return startDate && endDate && endDate < startDate ? { endBeforeStart: true } : null;
}

/** Opens the dialog to create a project, or to edit one when given. Resolves the saved project's ID, or undefined. */
export function openProjectDialog(dialog: Dialog, project?: ProjectSummary): Observable<Guid | undefined> {
  return dialog.open<Guid, ProjectSummary | undefined>(ProjectFormDialog, {
    data: project,
    maxWidth: 'calc(100vw - 32px)',
    backdropClass: 'cdk-overlay-dark-backdrop',
    ariaLabelledBy: 'project-form-title',
  }).closed;
}

@Component({
  selector: 'app-project-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, Icon, AiSuggest],
  templateUrl: './project-form-dialog.html',
})
export class ProjectFormDialog {
  private readonly store = inject(WorkspaceStore);
  private readonly ref = inject<DialogRef<Guid>>(DialogRef);
  /** Present when editing. */
  protected readonly project = inject<ProjectSummary | undefined>(DIALOG_DATA);

  protected readonly pending = signal(false);
  protected readonly formError = signal<string | null>(null);
  protected readonly confirmingDiscard = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group(
    {
      name: [this.project?.name ?? '', [Validators.required, Validators.maxLength(200)]],
      description: [this.project?.description ?? '', [Validators.maxLength(2000)]],
      startDate: [this.project?.startDate ? isoToLocalYmd(this.project.startDate) : ''],
      endDate: [this.project?.endDate ? isoToLocalYmd(this.project.endDate) : ''],
    },
    { validators: endNotBeforeStart },
  );

  constructor() {
    this.ref.disableClose = true;
    this.ref.backdropClick.pipe(takeUntilDestroyed()).subscribe(() => this.requestClose());
    this.ref.keydownEvents.pipe(takeUntilDestroyed()).subscribe((event) => {
      if (event.key === 'Escape') {
        this.requestClose();
      }
    });
  }

  /** AI content was applied in this edit; saved as isAiModified so the button stays hidden afterwards. */
  private readonly aiApplied = signal(false);
  protected readonly aiLocked = computed(() => this.project?.isAiModified === true || this.aiApplied());

  protected applyAi(suggestion: AiSuggestion): void {
    this.form.patchValue({ name: suggestion.title, description: suggestion.description });
    this.form.markAsDirty();
    // Shows a length error at once if the suggestion is longer than a project description allows.
    this.form.controls.name.markAsTouched();
    this.form.controls.description.markAsTouched();
    this.aiApplied.set(true);
  }

  protected requestClose(): void {
    if (this.form.dirty && !this.pending()) {
      this.confirmingDiscard.set(true);
    } else {
      this.ref.close();
    }
  }

  protected discard(): void {
    this.ref.close();
  }

  protected async submit(): Promise<void> {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.pending.set(true);
    try {
      const body = {
        name: value.name.trim(),
        description: value.description.trim() || null,
        startDate: value.startDate ? startOfLocalDayIso(value.startDate) : null,
        endDate: value.endDate ? endOfLocalDayIso(value.endDate) : null,
      };
      if (this.project) {
        // Kept true once set: the backend treats a missing flag as false, which would re-enable AI after a manual edit.
        await this.store.updateProject(this.project.id, { ...body, isAiModified: this.aiLocked() });
        this.ref.close(this.project.id);
      } else {
        this.ref.close((await this.store.createProject(body)).id);
      }
    } catch (error) {
      const appError = toAppError(error);
      if (appError.status === 400 || appError.status === 409) {
        const unmatched = applyServerErrors(this.form, appError.fieldErrors);
        this.formError.set(unmatched[0] ?? (Object.keys(appError.fieldErrors).length ? null : appError.message));
      }
    } finally {
      this.pending.set(false);
    }
  }
}
