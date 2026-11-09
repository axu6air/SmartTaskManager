import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { ToastService } from '@core/ui/toast.service';
import { catchError, finalize, switchMap, throwError } from 'rxjs';
import { AppError, toAppError } from './app-error';
import { SKIP_ERROR_TOAST, SKIP_LOADING } from './http-context';
import { LoadingService } from './loading.service';

/** Endpoints authenticated by their body (credentials or refresh token), never by a bearer token. */
const ANONYMOUS_PATHS = ['/auth/login', '/auth/refresh', '/auth/logout'];

function isAnonymous(req: HttpRequest<unknown>): boolean {
  return ANONYMOUS_PATHS.some((path) => req.url.endsWith(path));
}

function withToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

/** 1. Attaches the bearer token. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).accessToken();
  return token && !isAnonymous(req) ? next(withToken(req, token)) : next(req);
};

/** 2. On a 401, refreshes once (shared by every concurrent request) and retries; signs out if that fails. */
export const refreshTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      const unauthorized = error instanceof HttpErrorResponse && error.status === 401;
      if (!unauthorized) {
        return throwError(() => error);
      }
      if (isAnonymous(req) || !auth.hasRefreshToken()) {
        return throwError(() => toAppError(error));
      }

      return auth.refresh().pipe(
        catchError(() => {
          auth.endSession({ expired: true });
          return throwError(() => new AppError(401, 'Your session has expired. Please sign in again.'));
        }),
        switchMap((token) => next(withToken(req, token))),
        // A second 401 after a fresh token means the account itself was rejected.
        catchError((retryError: unknown) => throwError(() => toAppError(retryError))),
      );
    }),
  );
};

/** 3. Maps failures to AppError and reports the ones a caller can’t fix. 400s go back to the form. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: unknown) => {
      // The refresh interceptor still needs the raw 401 to decide whether to retry.
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return throwError(() => error);
      }

      const appError = toAppError(error);
      if (appError.status !== 400 && !req.context.get(SKIP_ERROR_TOAST)) {
        toast.error(appError.message);
      }
      return throwError(() => appError);
    }),
  );
};

/** 4. Drives the global progress bar. */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_LOADING)) {
    return next(req);
  }

  const loading = inject(LoadingService);
  loading.start();
  return next(req).pipe(finalize(() => loading.stop()));
};
