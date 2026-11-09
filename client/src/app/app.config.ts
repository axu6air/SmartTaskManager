import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { TitleStrategy, provideRouter, withComponentInputBinding, withPreloading } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import {
  authInterceptor,
  errorInterceptor,
  loadingInterceptor,
  refreshTokenInterceptor,
} from '@core/http/interceptors';
import { AppTitleStrategy, PermittedPreloading } from '@core/layout/routing-support';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding(), withPreloading(PermittedPreloading)),
    { provide: TitleStrategy, useClass: AppTitleStrategy },
    // Order matters: see "HTTP interceptors" in CLAUDE.md.
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, refreshTokenInterceptor, errorInterceptor, loadingInterceptor]),
    ),
    provideAppInitializer(() => inject(AuthService).restoreSession()),
  ],
};
