import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'check'
  | 'tick'
  | 'plus'
  | 'close'
  | 'search'
  | 'back'
  | 'chevron'
  | 'lock'
  | 'alert'
  | 'logout'
  | 'menu'
  | 'sun'
  | 'moon'
  | 'monitor'
  | 'edit'
  | 'trash'
  | 'grid'
  | 'board'
  | 'chart'
  | 'shield'
  | 'more'
  | 'spark'
  | 'userPlus';

/** Inline stroke icons that inherit the text colour. Always decorative: label the parent control. */
@Component({
  selector: 'app-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      [attr.stroke-width]="name() === 'tick' ? 3.5 : 2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      @switch (name()) {
        @case ('check') { <path d="M4 12l5 5L20 6" /> }
        @case ('tick') { <path d="M5 12l5 5L19 7" /> }
        @case ('plus') { <path d="M12 5v14M5 12h14" /> }
        @case ('close') { <path d="M6 6l12 12M18 6L6 18" /> }
        @case ('search') { <circle cx="11" cy="11" r="7" /><path d="M20 20l-3.5-3.5" /> }
        @case ('back') { <path d="M15 6l-6 6 6 6" /> }
        @case ('chevron') { <path d="M9 6l6 6-6 6" /> }
        @case ('lock') { <rect x="5" y="11" width="14" height="10" rx="2" /><path d="M8 11V7a4 4 0 018 0v4" /> }
        @case ('alert') { <circle cx="12" cy="12" r="9" /><path d="M12 8v5M12 16.5v.01" /> }
        @case ('logout') { <path d="M15 4h3a2 2 0 012 2v12a2 2 0 01-2 2h-3" /><path d="M10 17l-5-5 5-5M5 12h11" /> }
        @case ('menu') { <path d="M4 7h16M4 12h16M4 17h16" /> }
        @case ('sun') { <circle cx="12" cy="12" r="4" /><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" /> }
        @case ('moon') { <path d="M20 14.5A8 8 0 019.5 4a8 8 0 1010.5 10.5z" /> }
        @case ('monitor') { <rect x="3" y="4" width="18" height="12" rx="2" /><path d="M8 20h8M12 16v4" /> }
        @case ('edit') { <path d="M4 20h4L19 9l-4-4L4 16v4z" /><path d="M14 6l4 4" /> }
        @case ('trash') { <path d="M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3" /> }
        @case ('grid') { <rect x="4" y="4" width="7" height="7" rx="1.5" /><rect x="13" y="4" width="7" height="7" rx="1.5" /><rect x="4" y="13" width="7" height="7" rx="1.5" /><rect x="13" y="13" width="7" height="7" rx="1.5" /> }
        @case ('board') { <rect x="3" y="4" width="5" height="16" rx="1.5" /><rect x="10" y="4" width="5" height="11" rx="1.5" /><rect x="17" y="4" width="4" height="7" rx="1.5" /> }
        @case ('chart') { <path d="M4 20V10M10 20V4M16 20v-7M22 20H2" /> }
        @case ('more') { <circle cx="12" cy="5" r="1.6" fill="currentColor" /><circle cx="12" cy="12" r="1.6" fill="currentColor" /><circle cx="12" cy="19" r="1.6" fill="currentColor" /> }
        @case ('spark') { <path d="M12 3l1.8 4.7L18.5 9.5l-4.7 1.8L12 16l-1.8-4.7L5.5 9.5l4.7-1.8z" /><path d="M19 15l.8 2.2L22 18l-2.2.8L19 21l-.8-2.2L16 18l2.2-.8z" /> }
        @case ('userPlus') { <circle cx="9" cy="8" r="4" /><path d="M2 20c0-3.3 3.1-6 7-6s7 2.7 7 6M19 8v6M16 11h6" /> }
        @case ('shield') { <path d="M12 3l8 3v6c0 4.5-3.4 8.4-8 9-4.6-.6-8-4.5-8-9V6l8-3z" /><path d="M9 12l2 2 4-4" /> }
      }
    </svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      flex: none;
    }
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly size = input(16);
}
