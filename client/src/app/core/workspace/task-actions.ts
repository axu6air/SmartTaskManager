import { Dialog } from '@angular/cdk/dialog';
import { Overlay } from '@angular/cdk/overlay';
import { Injectable, inject } from '@angular/core';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { ConfirmDialogService } from '@core/ui/confirm-dialog.service';
import { ToastService } from '@core/ui/toast.service';
import { Guid } from '@models/api';
import { ProjectTask, STATUS_LABEL, TaskStatus } from '@models/task';
import { TaskDrawer } from './task-drawer';
import { TaskFormData, TaskFormDialog } from './task-form-dialog';
import { TaskView, TaskViews } from './task-views';
import { WorkspaceStore } from './workspace.store';

/** The task actions every page offers: open details, create, edit, move and delete. */
@Injectable({ providedIn: 'root' })
export class TaskActions {
  private readonly dialog = inject(Dialog);
  private readonly overlay = inject(Overlay);
  private readonly store = inject(WorkspaceStore);
  private readonly views = inject(TaskViews);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly toast = inject(ToastService);
  private readonly permissions = inject(PermissionService);

  /** Tasks are only fetched for users allowed to see them; the API would refuse otherwise. */
  reloadWorkspace(): void {
    void this.store.load({ includeTasks: this.permissions.has(Permissions.Tasks.View) });
  }

  openDetails(taskId: Guid): void {
    this.dialog.open(TaskDrawer, {
      data: taskId,
      maxWidth: '100vw',
      backdropClass: 'cdk-overlay-dark-backdrop',
      ariaLabelledBy: 'drawer-title',
      positionStrategy: this.overlay.position().global().top('0').right('0'),
    });
  }

  openCreate(defaultProjectId?: Guid): void {
    const projects = this.views.creatableProjects();
    if (projects.length === 0) {
      return;
    }
    this.openForm({
      projects,
      defaultProjectId: projects.some((p) => p.id === defaultProjectId) ? defaultProjectId : undefined,
      canAssign: true,
    }, 'Task created');
  }

  openEdit(view: TaskView): void {
    this.openForm({ task: view.task, projects: [], canAssign: view.canAssign }, 'Task updated');
  }

  async changeStatus(task: ProjectTask, status: TaskStatus): Promise<void> {
    try {
      await this.store.changeStatus(task, status);
      this.toast.success(`Moved to ${STATUS_LABEL[status]}`);
    } catch {
      // Rolled back by the store; the error interceptor already told the user why.
    }
  }

  /** Resolves true when the task was deleted. */
  async delete(task: ProjectTask): Promise<boolean> {
    const confirmed = await this.confirmDialog.confirm({
      title: 'Delete this task?',
      message: `“${task.title}” will be removed from the project.`,
      confirmLabel: 'Delete task',
      cancelLabel: 'Keep it',
      danger: true,
    });
    if (!confirmed) {
      return false;
    }
    try {
      await this.store.deleteTask(task);
      this.toast.success('Task deleted');
      return true;
    } catch {
      return false;
    }
  }

  private openForm(data: TaskFormData, successMessage: string): void {
    const ref = this.dialog.open<boolean, TaskFormData>(TaskFormDialog, {
      data,
      maxWidth: 'calc(100vw - 32px)',
      backdropClass: 'cdk-overlay-dark-backdrop',
      ariaLabelledBy: 'task-form-title',
    });
    ref.closed.subscribe((saved) => {
      if (saved) {
        this.toast.success(successMessage);
      }
    });
  }
}
