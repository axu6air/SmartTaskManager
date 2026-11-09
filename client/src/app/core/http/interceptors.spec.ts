import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '@core/auth/auth.service';
import { ToastService } from '@core/ui/toast.service';
import { Subject, firstValueFrom, share } from 'rxjs';
import { AppError } from './app-error';
import { authInterceptor, errorInterceptor, loadingInterceptor, refreshTokenInterceptor } from './interceptors';

describe('HTTP interceptors', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let refreshes: Subject<string>;
  const accessToken = signal<string | null>('old-token');
  const auth = {
    accessToken,
    hasRefreshToken: () => true,
    refresh: vi.fn(),
    endSession: vi.fn(),
  };
  const toast = { error: vi.fn(), info: vi.fn() };

  beforeEach(() => {
    accessToken.set('old-token');
    refreshes = new Subject<string>();
    const shared = refreshes.pipe(share());
    auth.refresh.mockReset().mockReturnValue(shared);
    auth.endSession.mockReset();
    toast.error.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor, refreshTokenInterceptor, errorInterceptor, loadingInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: ToastService, useValue: toast },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('attaches the bearer token, but not to auth endpoints', () => {
    http.get('/api/projects').subscribe();
    http.post('/api/auth/login', {}).subscribe();

    expect(backend.expectOne('/api/projects').request.headers.get('Authorization')).toBe('Bearer old-token');
    expect(backend.expectOne('/api/auth/login').request.headers.has('Authorization')).toBe(false);
  });

  it('on 401 refreshes once for concurrent requests and retries each with the new token', async () => {
    const first = firstValueFrom(http.get('/api/projects'));
    const second = firstValueFrom(http.get('/api/roles'));

    backend.expectOne('/api/projects').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/roles').flush(null, { status: 401, statusText: 'Unauthorized' });
    refreshes.next('new-token');
    refreshes.complete();

    const retried = backend.match((r) => r.headers.get('Authorization') === 'Bearer new-token');
    expect(retried.map((r) => r.request.url).sort()).toEqual(['/api/projects', '/api/roles']);
    retried.forEach((r) => r.flush({ success: true, data: 'ok' }));

    await expect(first).resolves.toBeTruthy();
    await expect(second).resolves.toBeTruthy();
    expect(auth.refresh).toHaveBeenCalledTimes(2);
  });

  it('ends the session when the refresh fails', async () => {
    const request = firstValueFrom(http.get('/api/projects'));

    backend.expectOne('/api/projects').flush(null, { status: 401, statusText: 'Unauthorized' });
    refreshes.error(new Error('refresh rejected'));

    await expect(request).rejects.toBeInstanceOf(AppError);
    expect(auth.endSession).toHaveBeenCalledWith({ expired: true });
  });

  it('toasts 403s but hands 400 validation errors back to the caller', async () => {
    const forbidden = firstValueFrom(http.get('/api/roles'));
    backend.expectOne('/api/roles').flush({ success: false, message: 'Nope', errors: null }, { status: 403, statusText: 'Forbidden' });
    await expect(forbidden).rejects.toMatchObject({ status: 403, message: 'Nope' });
    expect(toast.error).toHaveBeenCalledWith('Nope');

    toast.error.mockReset();
    const invalid = firstValueFrom(http.post('/api/projects/p/tasks', {}));
    backend
      .expectOne('/api/projects/p/tasks')
      .flush({ success: false, message: 'Invalid', errors: { Title: ['Required'] } }, { status: 400, statusText: 'Bad Request' });
    await expect(invalid).rejects.toMatchObject({ status: 400, fieldErrors: { Title: ['Required'] } });
    expect(toast.error).not.toHaveBeenCalled();
  });
});
