import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '@env/environment';
import { ApiResponse } from '@models/api';
import { Observable, map } from 'rxjs';

type Params = Record<string, string | number | boolean>;

interface Options {
  params?: Params;
  context?: HttpContext;
}

/** The only place that talks to HttpClient. Unwraps the ApiResponse envelope to its data. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  get<T>(path: string, options: Options = {}): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(this.url(path), this.httpOptions(options))
      .pipe(map(unwrap));
  }

  post<T>(path: string, body: unknown, options: Options = {}): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(this.url(path), body, this.httpOptions(options))
      .pipe(map(unwrap));
  }

  put<T>(path: string, body: unknown, options: Options = {}): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(this.url(path), body, this.httpOptions(options))
      .pipe(map(unwrap));
  }

  patch<T>(path: string, body: unknown, options: Options = {}): Observable<T> {
    return this.http
      .patch<ApiResponse<T>>(this.url(path), body, this.httpOptions(options))
      .pipe(map(unwrap));
  }

  /** For 204 No Content endpoints (delete, logout). */
  send(method: 'DELETE' | 'POST', path: string, body?: unknown, options: Options = {}): Observable<void> {
    return this.http
      .request(method, this.url(path), { body, ...this.httpOptions(options) })
      .pipe(map(() => undefined));
  }

  private url(path: string): string {
    return `${this.base}${path}`;
  }

  private httpOptions(options: Options): { params?: HttpParams; context?: HttpContext } {
    return {
      params: options.params ? new HttpParams({ fromObject: options.params }) : undefined,
      context: options.context,
    };
  }
}

function unwrap<T>(response: ApiResponse<T>): T {
  // A success envelope always carries data for these endpoints; null only appears on failures,
  // which never reach here because HttpClient routes non-2xx responses to the error channel.
  return response.data as T;
}
