import { DIALOG_DATA, Dialog, DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel?: string;
  danger?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="modal" role="alertdialog" aria-labelledby="confirm-title" aria-describedby="confirm-message">
      <h2 id="confirm-title">{{ data.title }}</h2>
      <p id="confirm-message" class="muted">{{ data.message }}</p>
      <div class="modal-actions">
        <button type="button" class="btn" (click)="ref.close(false)" cdkFocusInitial>
          {{ data.cancelLabel ?? 'Cancel' }}
        </button>
        <button
          type="button"
          class="btn"
          [class.btn-danger-solid]="data.danger"
          [class.btn-primary]="!data.danger"
          (click)="ref.close(true)"
        >
          {{ data.confirmLabel }}
        </button>
      </div>
    </div>
  `,
  styles: `
    .modal {
      width: 420px;
    }
  `,
})
class ConfirmDialog {
  protected readonly data = inject<ConfirmOptions>(DIALOG_DATA);
  protected readonly ref = inject<DialogRef<boolean>>(DialogRef);
}

/** Every destructive action asks here first. Focus starts on Cancel, so Enter never destroys by accident. */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly dialog = inject(Dialog);

  async confirm(options: ConfirmOptions): Promise<boolean> {
    const ref = this.dialog.open<boolean, ConfirmOptions>(ConfirmDialog, {
      data: options,
      maxWidth: 'calc(100vw - 32px)',
      backdropClass: 'cdk-overlay-dark-backdrop',
    });
    return (await firstValueFrom(ref.closed)) === true;
  }
}
