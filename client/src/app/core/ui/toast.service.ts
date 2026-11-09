import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  kind: 'info' | 'success' | 'error';
  message: string;
}

const DURATION_MS = 3500;
const MAX_VISIBLE = 3;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  readonly toasts = signal<Toast[]>([]);

  info(message: string): void {
    this.show('info', message);
  }

  success(message: string): void {
    this.show('success', message);
  }

  error(message: string): void {
    this.show('error', message);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private show(kind: Toast['kind'], message: string): void {
    // The same message twice in a row (e.g. several failed requests) shows once.
    if (this.toasts().some((t) => t.message === message)) {
      return;
    }
    const id = this.nextId++;
    this.toasts.update((list) => [...list, { id, kind, message }].slice(-MAX_VISIBLE));
    setTimeout(() => this.dismiss(id), DURATION_MS);
  }
}
