import { Router } from '@angular/router';

type QueryValue = string | number | boolean | null | undefined;

/**
 * Writes list state (filters, search, view) to the URL so refresh, back and shared links keep it.
 * Empty, false and default values are removed to keep URLs short. Replaces the history entry,
 * so typing in a search box doesn't add one entry per keystroke.
 */
export function updateQueryParams(router: Router, params: Record<string, QueryValue>): void {
  const cleaned = Object.fromEntries(
    Object.entries(params).map(([key, value]) => [key, value === '' || value === false ? null : value]),
  );
  void router.navigate([], { queryParams: cleaned, queryParamsHandling: 'merge', replaceUrl: true });
}
