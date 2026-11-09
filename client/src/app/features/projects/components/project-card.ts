import { CdkMenu, CdkMenuItem, CdkMenuTrigger } from '@angular/cdk/menu';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectMember, ProjectSummary } from '@models/project';
import { Avatar } from '@shared/ui/avatar';
import { Icon } from '@shared/ui/icon';
import { ProjectStats } from '../models/project-stats';

const MAX_AVATARS = 5;

@Component({
  selector: 'app-project-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Avatar, Icon, CdkMenuTrigger, CdkMenu, CdkMenuItem],
  template: `
    <article class="pcard">
      <div class="spread top">
        <div class="heading">
          <!-- The title link is stretched over the whole card, so the menu button is not nested inside a link. -->
          <h2>
            <a class="stretched" [routerLink]="['/projects', project().id]" [attr.aria-label]="'Open ' + project().name + ', ' + stats().open + ' open tasks'">
              {{ project().name }}
            </a>
          </h2>
          <p class="sum">{{ project().description || 'No description' }}</p>
        </div>
        @if (canEdit() || canDelete()) {
          <button
            type="button"
            class="icon-btn menu-btn"
            [cdkMenuTriggerFor]="menu"
            [attr.aria-label]="'Actions for ' + project().name"
          >
            <app-icon name="more" [size]="18" />
          </button>
        }
      </div>

      <div class="progress">
        <div class="spread">
          <span>{{ stats().done }} of {{ stats().countable }} tasks done</span>
          <b class="num">{{ stats().percentDone }}%</b>
        </div>
        <div class="bar"><i [style.width.%]="stats().percentDone"></i></div>
      </div>

      <div class="tags">
        <span class="tag">{{ stats().open }} open</span>
        @if (stats().overdue > 0) {
          <span class="tag tag-danger">{{ stats().overdue }} overdue</span>
        }
        @if (stats().mine > 0) {
          <span class="tag tag-accent">{{ stats().mine }} assigned to you</span>
        }
      </div>

      <div class="foot">
        <span class="avatars">
          @for (member of members().slice(0, maxAvatars); track member.userId) {
            <app-avatar [name]="member.fullName" [size]="30" [title]="member.fullName" />
          }
          @if (members().length > maxAvatars) {
            <span class="more">+{{ members().length - maxAvatars }}</span>
          }
        </span>
        <span class="go" aria-hidden="true">View tasks <app-icon name="chevron" /></span>
      </div>
    </article>

    <ng-template #menu>
      <div class="menu" cdkMenu>
        @if (canEdit()) {
          <button type="button" class="menu-item" cdkMenuItem (cdkMenuItemTriggered)="edit.emit()">
            <app-icon name="edit" /> Edit
          </button>
        }
        @if (canDelete()) {
          <button type="button" class="menu-item danger" cdkMenuItem (cdkMenuItemTriggered)="delete.emit()">
            <app-icon name="trash" /> Delete
          </button>
        }
      </div>
    </ng-template>
  `,
  styleUrl: './project-card.scss',
})
export class ProjectCard {
  readonly project = input.required<ProjectSummary>();
  readonly members = input.required<ProjectMember[]>();
  readonly stats = input.required<ProjectStats>();
  readonly canEdit = input(false);
  readonly canDelete = input(false);
  readonly edit = output();
  readonly delete = output();
  protected readonly maxAvatars = MAX_AVATARS;
}
