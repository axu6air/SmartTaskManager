import { TokenResponse } from '@models/auth';

const KEY = 'stm.session';

// shortcut: tokens in localStorage are readable by any script on the page (XSS); move the refresh
// token to an HttpOnly cookie when the API supports cookie-based refresh.
export function readTokens(): TokenResponse | null {
  try {
    const raw = globalThis.localStorage?.getItem(KEY);
    if (!raw) {
      return null;
    }
    const parsed = JSON.parse(raw) as Partial<TokenResponse>;
    return parsed.accessToken && parsed.refreshToken ? (parsed as TokenResponse) : null;
  } catch {
    return null;
  }
}

export function writeTokens(tokens: TokenResponse | null): void {
  try {
    if (tokens) {
      globalThis.localStorage?.setItem(KEY, JSON.stringify(tokens));
    } else {
      globalThis.localStorage?.removeItem(KEY);
    }
  } catch {
    // Storage blocked (private mode): the session still works until the tab closes.
  }
}
