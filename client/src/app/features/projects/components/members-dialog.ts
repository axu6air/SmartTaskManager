import { DIALOG_DATA, Dialog, DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { ToastService } from '@core/ui/toast.service';
import { ProjectApi } from '@core/workspace/project-api';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { Guid } from '@models/api';
import { ProjectMember, ProjectSummary, UserSummary } from '@models/project';
import { Avatar } from '@shared/ui/avatar';
import { Icon } from '@shared/ui/icon';
import { catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';

export function openMembersDialog(dialog: Dialog, project: ProjectSummary): void {
  dialog.open(MembersDialog, {
    data: project,
    maxWidth: 'calc(100vw - 32px)',
    backdropClass: 'cdk-overlay-dark-backdrop',
    ariaLabelledBy: 'members-title',
  });
}

/** Each change saves immediately; the server is the authority on what is allowed. */
@Component({
  selector: 'app-members-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Avatar, Icon],
  templateUrl: './members-dialog.html',
  styleUrl: './members-dialog.scss',
})
export class MembersDialog {
  protected readonly project = inject<ProjectSummary>(DIALOG_DATA);
  protected readonly ref = inject(DialogRef);
  private readonly store = inject(WorkspaceStore);
  private readonly api = inject(ProjectApi);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);

  protected readonly me = computed(() => this.auth.me()?.id);
  protected readonly canSearch = computed(() => {
    this.auth.permissions();
    return this.permissions.has(Permissions.Users.View);
  });

  protected readonly members = computed(() => this.store.members()[this.project.id] ?? []);
  private readonly managerCount = computed(() => this.members().filter((m) => m.isManager).length);
  /** The domain keeps at least one manager; the UI says so up front instead of failing. */
  protected readonly isLastManager = (m: ProjectMember) => m.isManager && this.managerCount() === 1;

  protected readonly busy = signal<Guid | null>(null);
  protected readonly confirmingRemove = signal<ProjectMember | null>(null);
  protected readonly assignedCount = computed(() => {
    const member = this.confirmingRemove();
    return member
      ? this.store.tasks().filter((t) => t.projectId === this.project.id && t.assigneeId === member.userId).length
      : 0;
  });

  protected readonly query = signal('');
  private readonly results = toSignal(
    toObservable(this.query).pipe(
      map((q) => q.trim()),
      debounceTime(300),
      distinctUntilChanged(),
      // switchMap drops stale responses; an empty query lists the first users alphabetically.
      switchMap((q) => (this.canSearch() ? this.api.searchUsers(q).pipe(catchError(() => of(null))) : of([]))),
    ),
    { initialValue: undefined },
  );
  protected readonly searchFailed = computed(() => this.results() === null);
  protected readonly candidates = computed<UserSummary[]>(() => {
    const memberIds = new Set(this.members().map((m) => m.userId));
    return (this.results() ?? []).filter((u) => !memberIds.has(u.id));
  });
  protected readonly searching = computed(() => this.canSearch() && this.results() === undefined);

  protected onSearch(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  protected async add(user: UserSummary): Promise<void> {
    await this.run(user.id, () => this.store.setMember(this.project.id, user.id, false), `${user.fullName} added`);
  }

  protected async setRole(member: ProjectMember, event: Event): Promise<void> {
    const select = event.target as HTMLSelectElement;
    const isManager = select.value === 'manager';
    const ok = await this.run(
      member.userId,
      () => this.store.setMember(this.project.id, member.userId, isManager),
      `${member.fullName} is now ${isManager ? 'a manager' : 'a member'}`,
    );
    if (!ok) {
      // The bound value didn't change, so Angular won't reset the element; do it here.
      select.value = member.isManager ? 'manager' : 'member';
    }
  }

  protected async remove(member: ProjectMember): Promise<void> {
    this.confirmingRemove.set(null);
    await this.run(member.userId, () => this.store.removeMember(this.project.id, member.userId), `${member.fullName} removed`);
  }

  private async run(userId: Guid, action: () => Promise<void>, success: string): Promise<boolean> {
    this.busy.set(userId);
    try {
      await action();
      this.toast.success(success);
      return true;
    } catch {
      // The error interceptor shows the server's reason (403, last manager, ...).
      return false;
    } finally {
      this.busy.set(null);
    }
  }
}
