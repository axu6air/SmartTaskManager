import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

const PALETTE_SIZE = 6;

/** Initials on a colour derived from the name, so a person keeps the same colour everywhere. */
@Component({
  selector: 'app-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `{{ initials() }}`,
  host: {
    'aria-hidden': 'true',
    '[style.width.px]': 'size()',
    '[style.height.px]': 'size()',
    '[style.font-size.px]': 'size() * 0.42',
    '[style.background]': 'color()',
  },
  styles: `
    :host {
      border-radius: 50%;
      color: var(--on-avatar);
      font-weight: 600;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex: none;
      line-height: 1;
      user-select: none;
    }
  `,
})
export class Avatar {
  readonly name = input<string | null | undefined>();
  readonly size = input(28);

  protected readonly initials = computed(() => {
    const parts = (this.name() ?? '').trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) {
      return '?';
    }
    const first = parts[0][0];
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  });

  protected readonly color = computed(() => {
    const name = this.name();
    if (!name) {
      return 'var(--muted)';
    }
    let hash = 0;
    for (const char of name) {
      hash = (hash * 31 + char.charCodeAt(0)) | 0;
    }
    return `var(--avatar-${(Math.abs(hash) % PALETTE_SIZE) + 1})`;
  });
}
