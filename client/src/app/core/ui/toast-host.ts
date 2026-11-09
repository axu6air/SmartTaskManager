import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-host',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toasts" role="status" aria-live="polite">
      @for (toast of toasts.toasts(); track toast.id) {
        <div class="toast" [class.error]="toast.kind === 'error'" [class.success]="toast.kind === 'success'">
          <span>{{ toast.message }}</span>
          <button type="button" class="close" (click)="toasts.dismiss(toast.id)" aria-label="Dismiss">
            ×
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts {
      position: fixed;
      left: 50%;
      bottom: calc(var(--space-6) + env(safe-area-inset-bottom, 0px));
      transform: translateX(-50%);
      z-index: 1100;
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      width: max-content;
      max-width: calc(100vw - 32px);
    }
    .toast {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      background: var(--fg);
      color: var(--bg);
      padding: 10px var(--space-2) 10px 18px;
      border-radius: 10px;
      font-weight: 500;
      box-shadow: var(--shadow-lg);
      animation: rise 0.18s ease-out;
    }
    .toast.error {
      background: var(--danger);
      color: var(--on-accent);
    }
    .toast.success {
      background: var(--ok);
      color: var(--on-accent);
    }
    .close {
      border: none;
      background: transparent;
      color: inherit;
      font-size: 20px;
      line-height: 1;
      width: 32px;
      height: 32px;
      border-radius: 6px;
      opacity: 0.8;
    }
    @keyframes rise {
      from {
        transform: translateY(8px);
        opacity: 0;
      }
    }
  `,
})
export class ToastHost {
  protected readonly toasts = inject(ToastService);
}
