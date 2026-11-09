import { Dialog } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { ConfirmDialogService } from '@core/ui/confirm-dialog.service';
import { ToastService } from '@core/ui/toast.service';
import { ProjectSummary } from '@models/project';
import { openProjectDialog } from '../components/project-form-dialog';
import { HasPermissionDirective } from '@core/auth/has-permission.directive';
import { Permissions } from '@core/auth/permissions';
import { PermissionService } from '@core/auth/permission.service';
import { TaskActions } from '@core/workspace/task-actions';
import { TaskViews } from '@core/workspace/task-views';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { PermissionScope } from '@models/scope';
import { Icon } from '@shared/ui/icon';
import { LoadError } from '@shared/ui/load-error';
import { ProjectCard } from '../components/project-card';
import { projectStats } from '../models/project-stats';

@Component({
  selector: 'app-project-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProjectCard, LoadError, Icon, HasPermissionDirective],
  template: `
    <div class="page">
      <header class="page-head">
        <div>
          <h1>Projects</h1>
          <p>{{ subtitle() }}</p>
        </div>
        <div class="actions">
          <button type="button" class="btn" *appHasPermission="perms.Projects.Create" (click)="createProject()">
            <app-icon name="plus" /> New project
          </button>
          <button type="button" class="btn btn-primary" *appHasPermission="perms.Tasks.Create" (click)="actions.openCreate()" [disabled]="views.creatableProjects().length === 0">
            <app-icon name="plus" /> New task
          </button>
        </div>
      </header>

      @switch (store.status()) {
        @case ('error') {
          <app-load-error [message]="store.error() ?? ''" (retry)="reload()" />
        }
        @case ('loaded') {
          @if (cards().length === 0) {
            <section class="card state">
              <h2>No projects yet</h2>
              <p>You aren’t a member of any project yet.</p>
              <button type="button" class="btn btn-primary" *appHasPermission="perms.Projects.Create" (click)="createProject()">
                <app-icon name="plus" /> Create your first project
              </button>
            </section>
          } @else {
            <div class="grid">
              @for (card of cards(); track card.project.id) {
                <app-project-card
                  [project]="card.project"
                  [members]="card.members"
                  [stats]="card.stats"
                  [canEdit]="card.canEdit"
                  [canDelete]="card.canDelete"
                  (edit)="editProject(card.project)"
                  (delete)="deleteProject(card.project, card.stats.total)"
                />
              }
            </div>
          }
        }
        @default {
          <div class="grid" aria-busy="true" aria-label="Loading projects">
            @for (i of skeletons; track i) {
              <div class="card skeleton-card"><span class="skeleton"></span><span class="skeleton"></span><span class="skeleton"></span></div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: `
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(min(300px, 100%), 1fr));
      gap: var(--space-4);
    }
    .skeleton-card {
      padding: var(--space-5);
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
      min-height: 230px;
    }
    .skeleton:nth-child(1) { height: 22px; width: 60%; }
    .skeleton:nth-child(2) { height: 8px; }
    .skeleton:nth-child(3) { height: 26px; width: 45%; }
  `,
})
export class ProjectList {
  protected readonly store = inject(WorkspaceStore);
  protected readonly views = inject(TaskViews);
  protected readonly actions = inject(TaskActions);
  private readonly permissions = inject(PermissionService);
  private readonly dialog = inject(Dialog);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly auth = inject(AuthService);
  protected readonly perms = Permissions;
  protected readonly skeletons = [1, 2, 3];

  protected readonly subtitle = computed(() =>
    (this.permissions.scopeOf(Permissions.Projects.View) & PermissionScope.All) !== 0
      ? 'Every project in the workspace. Pick one to see its tasks.'
      : 'Projects you’re a member of. Pick one to see its tasks.',
  );

  protected readonly cards = computed(() => {
    this.auth.permissions();
    const members = this.store.members();
    const all = this.views.all();
    return this.store.projects().map((project) => {
      const projectMembers = members[project.id] ?? [];
      return {
        project,
        members: projectMembers,
        stats: projectStats(all.filter((v) => v.task.projectId === project.id)),
        // Scope-aware, like the server: a manager may only edit or delete projects they manage.
        canEdit: this.permissions.canOnProject(Permissions.Projects.Update, projectMembers),
        canDelete: this.permissions.canOnProject(Permissions.Projects.Delete, projectMembers),
      };
    });
  });

  protected editProject(project: ProjectSummary): void {
    openProjectDialog(this.dialog, project).subscribe((saved) => {
      if (saved) {
        this.toast.success('Project updated');
      }
    });
  }

  protected async deleteProject(project: ProjectSummary, taskCount: number): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      title: `Delete “${project.name}”?`,
      message:
        taskCount > 0
          ? `The project and its ${taskCount} ${taskCount === 1 ? 'task' : 'tasks'} will be removed for everyone.`
          : 'The project will be removed for everyone.',
      confirmLabel: 'Delete project',
      cancelLabel: 'Keep it',
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.store.deleteProject(project.id);
      this.toast.success('Project deleted');
    } catch {
      // The error interceptor already showed why.
    }
  }

  protected createProject(): void {
    openProjectDialog(this.dialog).subscribe((projectId) => {
      if (projectId) {
        this.toast.success('Project created');
        void this.router.navigate(['/projects', projectId]);
      }
    });
  }

  protected reload(): void {
    this.actions.reloadWorkspace();
  }
}
