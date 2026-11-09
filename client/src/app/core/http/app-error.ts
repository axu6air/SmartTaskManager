import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from '@models/api';

/** Every failed HTTP call surfaces as an AppError. */
export class AppError extends Error {
  constructor(
    readonly status: number,
    message: string,
    /** Validation messages by field name, as sent by the server (PascalCase keys, e.g. "DueDate"). */
    readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'AppError';
  }
}

const FALLBACK: Record<number, string> = {
  0: 'Can’t reach the server. Check your connection and try again.',
  401: 'Your session has expired. Please sign in again.',
  403: 'You don’t have permission to do that.',
  404: 'That item no longer exists.',
};

function isEnvelope(body: unknown): body is ApiResponse<unknown> {
  return typeof body === 'object' && body !== null && 'success' in body;
}

export function toAppError(error: unknown): AppError {
  if (error instanceof AppError) {
    return error;
  }

  if (!(error instanceof HttpErrorResponse)) {
    return new AppError(0, 'Something went wrong. Please try again.');
  }

  const body = isEnvelope(error.error) ? error.error : null;
  const message =
    body?.message ?? FALLBACK[error.status] ?? 'Something went wrong on our side. Please try again.';

  return new AppError(error.status, message, body?.errors ?? {});
}
