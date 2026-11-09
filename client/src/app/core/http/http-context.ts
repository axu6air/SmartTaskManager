import { HttpContextToken } from '@angular/common/http';

/** Keeps a request out of the global loading indicator (background refreshes, polling). */
export const SKIP_LOADING = new HttpContextToken<boolean>(() => false);

/** Lets the caller show its own message instead of the global error toast. */
export const SKIP_ERROR_TOAST = new HttpContextToken<boolean>(() => false);
