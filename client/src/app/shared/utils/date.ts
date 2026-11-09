import { OffsetDateString } from '@models/api';

/**
 * The only place dates are parsed or built. The API speaks DateTimeOffset, so every value sent keeps
 * the user's local offset (never toISOString(), which silently converts to UTC and loses the day).
 */

function pad(n: number, width = 2): string {
  return String(Math.abs(n)).padStart(width, '0');
}

/** "2026-11-15" for the local calendar day of the date. */
export function toLocalYmd(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

export function todayYmd(now: Date = new Date()): string {
  return toLocalYmd(now);
}

/** "2026-11-15T23:59:59.000+06:00": the local wall time with the local offset. */
export function toOffsetIso(date: Date): OffsetDateString {
  const offsetMinutes = -date.getTimezoneOffset();
  const sign = offsetMinutes >= 0 ? '+' : '-';
  const offset = `${sign}${pad(Math.trunc(offsetMinutes / 60))}:${pad(offsetMinutes % 60)}`;
  return (
    `${toLocalYmd(date)}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}` +
    `.${pad(date.getMilliseconds(), 3)}${offset}`
  );
}

/**
 * A date-input value ("2026-11-15") as the last moment of that local day, so a task due "today"
 * stays valid all day and passes the server's "not in the past" rule.
 */
export function endOfLocalDayIso(ymd: string): OffsetDateString {
  const [year, month, day] = ymd.split('-').map(Number);
  return toOffsetIso(new Date(year, month - 1, day, 23, 59, 59, 999));
}

/** A date-input value as the first moment of that local day. */
export function startOfLocalDayIso(ymd: string): OffsetDateString {
  const [year, month, day] = ymd.split('-').map(Number);
  return toOffsetIso(new Date(year, month - 1, day));
}

/** The local calendar day of an offset timestamp, for date inputs and day comparisons. */
export function isoToLocalYmd(iso: OffsetDateString): string {
  return toLocalYmd(new Date(iso));
}

/** "15 Nov", or "15 Nov 2027" outside the current year. */
export function formatShortDate(iso: OffsetDateString, now: Date = new Date()): string {
  const date = new Date(iso);
  return date.toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    ...(date.getFullYear() === now.getFullYear() ? {} : { year: 'numeric' }),
  });
}

export type DueTone = 'none' | 'normal' | 'today' | 'overdue';

export interface DueInfo {
  label: string;
  tone: DueTone;
}

/** Compared by local calendar day, so "overdue" flips at the user's midnight, not UTC's. */
export function dueInfo(
  due: OffsetDateString | null,
  closed: boolean,
  now: Date = new Date(),
): DueInfo {
  if (!due) {
    return { label: 'No due date', tone: 'none' };
  }

  const label = formatShortDate(due, now);
  if (closed) {
    return { label: `Due ${label}`, tone: 'normal' };
  }

  const dueDay = isoToLocalYmd(due);
  const today = todayYmd(now);
  if (dueDay < today) {
    return { label: `Overdue · ${label}`, tone: 'overdue' };
  }
  if (dueDay === today) {
    return { label: 'Due today', tone: 'today' };
  }
  return { label: `Due ${label}`, tone: 'normal' };
}
