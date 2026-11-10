import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { PermissionService } from '@core/auth/permission.service';
import { Permissions } from '@core/auth/permissions';
import { AiApi } from '@core/workspace/ai-api';
import { AiSuggestion } from '@models/task';
import { Icon } from '@shared/ui/icon';
import { firstValueFrom } from 'rxjs';

/**
 * "Improve with AI" for an edit form: asks for a suggestion, previews it, and emits it only when the
 * user applies it. Renders nothing for users without the AI permission.
 */
@Component({
  selector: 'app-ai-suggest',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  template: `
    @if (allowed()) {
      @if (suggestion(); as s) {
        <div class="ai-box" aria-live="polite">
          <span class="label">AI suggestion · preview</span>
          <b class="ai-title">{{ s.title }}</b>
          <p class="ai-desc">{{ s.description }}</p>
          <div class="actions">
            <button type="button" class="btn btn-sm btn-primary" (click)="applySuggestion(s)">Apply</button>
            <button type="button" class="btn btn-sm" (click)="suggestion.set(null)">Discard</button>
          </div>
        </div>
      } @else {
        <div class="row">
          <button
            type="button"
            class="btn btn-sm ai-btn"
            [disabled]="loading() || !description().trim()"
            [title]="description().trim() ? '' : 'Write a description first'"
            (click)="improve()"
          >
            @if (loading()) {
              <span class="spin"></span> Improving…
            } @else {
              <app-icon name="spark" /> Improve with AI
            }
          </button>
        </div>
      }
    }
  `,
  styles: `
    .row {
      display: flex;
      justify-content: flex-end;
    }
    .ai-btn {
      border-color: var(--accent);
      color: var(--accent-ink);
      background: var(--accent-weak);
    }
    .ai-box {
      border: 1px solid var(--accent);
      background: var(--accent-weak);
      border-radius: 10px;
      padding: 14px;
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    .ai-box .label {
      color: var(--accent-ink);
    }
    .ai-desc {
      white-space: pre-line;
      line-height: 1.6;
      max-height: 220px;
      overflow-y: auto;
      overflow-wrap: anywhere;
    }
  `,
})
export class AiSuggest {
  private readonly ai = inject(AiApi);
  private readonly auth = inject(AuthService);
  private readonly permissions = inject(PermissionService);

  /** The form's current values, sent as the AI's input. */
  readonly title = input('');
  readonly description = input('');
  readonly apply = output<AiSuggestion>();

  protected readonly allowed = computed(() => {
    this.auth.permissions();
    return this.permissions.has(Permissions.Ai.EnhanceDescription);
  });
  protected readonly loading = signal(false);
  protected readonly suggestion = signal<AiSuggestion | null>(null);

  protected async improve(): Promise<void> {
    this.loading.set(true);
    try {
      this.suggestion.set(await firstValueFrom(this.ai.improve(this.title().trim() || null, this.description().trim())));
    } catch {
      // The error interceptor shows why (e.g. 503 when the AI service is unavailable).
    } finally {
      this.loading.set(false);
    }
  }

  protected applySuggestion(suggestion: AiSuggestion): void {
    this.suggestion.set(null);
    this.apply.emit(suggestion);
  }
}
