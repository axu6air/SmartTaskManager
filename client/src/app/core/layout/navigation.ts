import { Permissions, PermissionCode } from '@core/auth/permissions';
import { IconName } from '@shared/ui/icon';

export interface NavItem {
  path: string;
  label: string;
  icon: IconName;
  /** Shown when the user holds ANY of these; the route guard uses the same list. */
  permissions: readonly PermissionCode[];
  badge?: 'projects' | 'myOpenTasks';
}

export const NAV_ITEMS: readonly NavItem[] = [
  { path: '/projects', label: 'Projects', icon: 'grid', permissions: [Permissions.Projects.View], badge: 'projects' },
  { path: '/dashboard', label: 'Dashboard', icon: 'chart', permissions: [Permissions.Dashboard.View] },
  { path: '/board', label: 'Task board', icon: 'board', permissions: [Permissions.Tasks.View], badge: 'myOpenTasks' },
  { path: '/roles', label: 'Roles & permissions', icon: 'shield', permissions: [Permissions.Roles.View] },
];
