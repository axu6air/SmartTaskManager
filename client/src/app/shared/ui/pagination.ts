import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Icon } from './icon';

/** "1–10 of 57" with previous/next. Hidden entirely when everything fits on one page. */
@Component({
  selector: 'app-pagination',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  template: `
    @if (totalPages() > 1) {
      <nav class="pager" [attr.aria-label]="label()">
        <span class="range num" aria-live="polite">{{ from() }}–{{ to() }} of {{ total() }}</span>
        <div class="buttons">
          <button type="button" class="btn btn-sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)" aria-label="Previous page">
            <app-icon name="back" /> Prev
          </button>
          <span class="current num">Page {{ page() }} of {{ totalPages() }}</span>
          <button type="button" class="btn btn-sm" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)" aria-label="Next page">
            Next <app-icon name="chevron" />
          </button>
        </div>
      </nav>
    }
  `,
  styles: `
    .pager {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      font-size: 13px;
      color: var(--muted);
    }
    .buttons {
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }
    .current {
      min-width: 90px;
      text-align: center;
    }
  `,
})
export class Pagination {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly total = input.required<number>();
  readonly label = input('Pagination');
  readonly pageChange = output<number>();

  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  protected readonly from = computed(() => (this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1));
  protected readonly to = computed(() => Math.min(this.page() * this.pageSize(), this.total()));
}
