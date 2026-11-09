import { HttpContext } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@core/http/api.service';
import { SKIP_ERROR_TOAST, SKIP_LOADING } from '@core/http/http-context';
import { ToastService } from '@core/ui/toast.service';
import { WorkspaceStore } from '@core/workspace/workspace.store';
import { Me, TokenResponse } from '@models/auth';
import { Observable, finalize, firstValueFrom, map, shareReplay } from 'rxjs';
import { readTokens, writeTokens } from './token-storage';

const QUIET = () => new HttpContext().set(SKIP_ERROR_TOAST, true).set(SKIP_LOADING, true);

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly workspace = inject(WorkspaceStore);

  private readonly tokens = signal<TokenResponse | null>(readTokens());
  private inFlightRefresh: Observable<string> | null = null;

  readonly me = signal<Me | null>(null);
  readonly accessToken = computed(() => this.tokens()?.accessToken ?? null);
  readonly isAuthenticated = computed(() => this.me() !== null);
  readonly permissions = computed<Record<string, number>>(() => this.me()?.permissions ?? {});

  hasRefreshToken(): boolean {
    return this.tokens() !== null;
  }

  async login(email: string, password: string): Promise<void> {
    const tokens = await firstValueFrom(
      this.api.post<TokenResponse>('/auth/login', { email, password }),
    );
    this.setTokens(tokens);
    await this.loadMe();
  }

  /** Re-reads profile and permissions, e.g. after the user’s role grants changed. */
  async loadMe(): Promise<void> {
    this.me.set(await firstValueFrom(this.api.get<Me>('/auth/me')));
  }

  /** On app start: resume a stored session, or start signed out. */
  async restoreSession(): Promise<void> {
    if (!this.tokens()) {
      return;
    }
    try {
      await this.loadMe();
    } catch {
      this.clear();
    }
  }

  /** One refresh at a time: concurrent 401s all wait on the same request. */
  refresh(): Observable<string> {
    const refreshToken = this.tokens()?.refreshToken;
    if (!refreshToken) {
      throw new Error('No refresh token.');
    }

    this.inFlightRefresh ??= this.api
      .post<TokenResponse>('/auth/refresh', { refreshToken }, { context: QUIET() })
      .pipe(
        map((tokens) => {
          this.setTokens(tokens);
          return tokens.accessToken;
        }),
        finalize(() => (this.inFlightRefresh = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.inFlightRefresh;
  }

  async logout(): Promise<void> {
    const refreshToken = this.tokens()?.refreshToken;
    if (refreshToken) {
      // Best effort: a failed revoke must not keep the user signed in locally.
      await firstValueFrom(
        this.api.send('POST', '/auth/logout', { refreshToken }, { context: QUIET() }),
      ).catch(() => undefined);
    }
    this.endSession({ expired: false });
  }

  endSession({ expired }: { expired: boolean }): void {
    const returnUrl = expired ? this.router.url : undefined;
    this.clear();
    if (expired) {
      this.toast.info('Your session has expired. Please sign in again.');
    }
    void this.router.navigate(['/login'], {
      queryParams: returnUrl && returnUrl !== '/login' ? { returnUrl } : {},
    });
  }

  private clear(): void {
    this.setTokens(null);
    this.me.set(null);
    this.workspace.reset();
  }

  private setTokens(tokens: TokenResponse | null): void {
    this.tokens.set(tokens);
    writeTokens(tokens);
  }
}
