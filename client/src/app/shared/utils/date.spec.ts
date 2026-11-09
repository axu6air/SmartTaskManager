import { dueInfo, endOfLocalDayIso, isoToLocalYmd, toOffsetIso } from './date';

describe('date utils', () => {
  const now = new Date(2026, 10, 8, 14, 30); // 8 Nov 2026, 14:30 local

  it('toOffsetIso keeps local wall time and appends the local offset', () => {
    const iso = toOffsetIso(new Date(2026, 10, 15, 9, 5, 7, 42));

    expect(iso.startsWith('2026-11-15T09:05:07.042')).toBe(true);
    expect(iso).toMatch(/[+-]\d{2}:\d{2}$/);
    expect(new Date(iso).getTime()).toBe(new Date(2026, 10, 15, 9, 5, 7, 42).getTime());
  });

  it('endOfLocalDayIso is the last moment of that local day', () => {
    const iso = endOfLocalDayIso('2026-11-15');

    expect(isoToLocalYmd(iso)).toBe('2026-11-15');
    expect(new Date(iso).getHours()).toBe(23);
  });

  it('dueInfo flags overdue by local calendar day', () => {
    expect(dueInfo(endOfLocalDayIso('2026-11-07'), false, now).tone).toBe('overdue');
    expect(dueInfo(endOfLocalDayIso('2026-11-08'), false, now)).toEqual({ label: 'Due today', tone: 'today' });
    expect(dueInfo(endOfLocalDayIso('2026-11-09'), false, now).tone).toBe('normal');
  });

  it('dueInfo never marks closed tasks overdue', () => {
    expect(dueInfo(endOfLocalDayIso('2026-11-01'), true, now).tone).toBe('normal');
  });

  it('dueInfo handles a missing date', () => {
    expect(dueInfo(null, false, now)).toEqual({ label: 'No due date', tone: 'none' });
  });

  it('compares a timestamp from another offset by the user’s local day', () => {
    const sameInstantElsewhere = new Date(2026, 10, 9, 0, 30).toISOString(); // UTC form of local 9 Nov 00:30

    expect(isoToLocalYmd(sameInstantElsewhere)).toBe('2026-11-09');
  });
});
