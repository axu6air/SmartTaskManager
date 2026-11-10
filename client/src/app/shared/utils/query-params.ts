import { Router } from '@angular/router';

type QueryValue = string | number | boolean | null | undefined;

/** HTTP query params from a list query, dropping undefined, null and blank values. */
export function toParams(query: object): Record<string, string | number | boolean> {
  return Object.fromEntries(
    Object.entries(query).filter(
      (entry): entry is [string, string | number | boolean] =>
        entry[1] !== undefined && entry[1] !== null && !(typeof entry[1] === 'string' && entry[1].trim() === ''),
    ),
  );
}

/** A positive integer page number from a query param; anything else is page 1. */
export function pageFrom(value: string | undefined): number {
  const page = Number(value);
  return Number.isInteger(page) && page > 0 ? page : 1;
}

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
