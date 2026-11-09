import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';

describe('AuthService.refresh', () => {
  beforeEach(() => {
    localStorage.setItem(
      'stm.session',
      JSON.stringify({ accessToken: 'a1', refreshToken: 'r1', accessTokenExpiresAt: '2026-01-01T00:00:00+00:00' }),
    );
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  afterEach(() => localStorage.clear());

  it('shares one in-flight request between concurrent callers and stores the rotated tokens', async () => {
    const auth = TestBed.inject(AuthService);
    const backend = TestBed.inject(HttpTestingController);

    const first = firstValueFrom(auth.refresh());
    const second = firstValueFrom(auth.refresh());

    const request = backend.expectOne('/api/auth/refresh');
    expect(request.request.body).toEqual({ refreshToken: 'r1' });
    request.flush({
      success: true,
      data: { accessToken: 'a2', refreshToken: 'r2', accessTokenExpiresAt: '2026-01-01T00:15:00+00:00' },
    });

    await expect(first).resolves.toBe('a2');
    await expect(second).resolves.toBe('a2');
    expect(auth.accessToken()).toBe('a2');
    expect(JSON.parse(localStorage.getItem('stm.session') ?? '{}').refreshToken).toBe('r2');
    backend.verify();
  });
});
