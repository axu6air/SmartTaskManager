import { DOCUMENT } from '@angular/common';
import { Injectable, effect, inject, signal } from '@angular/core';

export type ThemeChoice = 'system' | 'light' | 'dark';

const KEY = 'stm.theme';
const ORDER: ThemeChoice[] = ['system', 'light', 'dark'];

function readChoice(): ThemeChoice {
  try {
    const value = globalThis.localStorage?.getItem(KEY);
    return value === 'light' || value === 'dark' ? value : 'system';
  } catch {
    return 'system';
  }
}

/** "system" follows the OS; light/dark stamp data-theme on <html>, which the tokens in styles.scss read. */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly choice = signal<ThemeChoice>(readChoice());

  constructor() {
    effect(() => {
      const choice = this.choice();
      const root = this.document.documentElement;
      if (choice === 'system') {
        root.removeAttribute('data-theme');
      } else {
        root.setAttribute('data-theme', choice);
      }
      try {
        globalThis.localStorage?.setItem(KEY, choice);
      } catch {
        // Storage blocked: the choice lasts for this tab only.
      }
    });
  }

  cycle(): void {
    this.choice.update((current) => ORDER[(ORDER.indexOf(current) + 1) % ORDER.length]);
  }
}
