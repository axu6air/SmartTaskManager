/** A GUID from the backend, kept as its string form. */
export type Guid = string;

/** An ISO 8601 date-time string with offset, from a backend DateTimeOffset. */
export type OffsetDateString = string;

/** Mirrors SmartTaskManager.Api.Common.ApiResponse<T>: every response body uses this envelope. */
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: Record<string, string[]> | null;
  traceId: string;
}

export type SortDirection = 'asc' | 'desc';

/** Query parameters shared by the paged list endpoints. Empty values are left out of the request. */
export interface ListQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: SortDirection;
}

/** Mirrors SmartTaskManager.Application.Common.PagedResponse<T>. */
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
