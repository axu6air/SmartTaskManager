/** Mirrors the PermissionScopes flags enum. Flags combine with bitwise OR; they are not a hierarchy. */
export const PermissionScope = {
  Assigned: 1,
  Managed: 2,
  Member: 4,
  All: 8,
} as const;

export type PermissionScopeFlag = (typeof PermissionScope)[keyof typeof PermissionScope];

const SCOPE_NAMES: [number, string][] = [
  [PermissionScope.All, 'All'],
  [PermissionScope.Member, 'Member'],
  [PermissionScope.Managed, 'Managed'],
  [PermissionScope.Assigned, 'Assigned'],
];

/** "Assigned + Managed" for 3, "All" for 8. */
export function scopeLabel(scope: number): string {
  return SCOPE_NAMES.filter(([flag]) => (scope & flag) !== 0)
    .map(([, name]) => name)
    .reverse()
    .join(' + ');
}
