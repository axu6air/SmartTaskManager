import { linkedSignal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { PagedResponse } from '@models/api';
import { Observable } from 'rxjs';

/**
 * A server-paged list that refetches whenever `params` changes (stale requests are cancelled) and
 * keeps showing the previous page while the next one loads, so the table doesn't flash empty.
 * Call it in an injection context (a field initializer).
 */
export function pagedResource<P extends object, T>(
  params: () => P,
  load: (params: P) => Observable<PagedResponse<T>>,
) {
  const resource = rxResource({ params, stream: ({ params }) => load(params) });

  const data = linkedSignal<PagedResponse<T> | undefined, PagedResponse<T> | undefined>({
    source: () => (resource.hasValue() ? resource.value() : undefined),
    computation: (value, previous) => value ?? previous?.value,
  });

  return {
    data,
    loading: resource.isLoading,
    error: resource.error,
    reload: () => resource.reload(),
  };
}
