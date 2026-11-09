import { Routes } from '@angular/router';
import { authGuard, guestGuard, permissionGuard } from '@core/auth/guards';
import { Permissions } from '@core/auth/permissions';
import { homePath } from '@core/layout/routing-support';
import { Shell } from '@core/layout/shell';
import { StatusPage } from '@core/layout/status-page';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in',
    canMatch: [guestGuard],
    loadComponent: () => import('@features/auth/login').then((m) => m.Login),
  },
  {
    path: '',
    component: Shell,
    canMatch: [authGuard],
    children: [
      {
        path: 'projects',
        canMatch: [permissionGuard],
        data: { permissions: [Permissions.Projects.View] },
        loadChildren: () => import('@features/projects/projects.routes').then((m) => m.PROJECT_ROUTES),
      },
      {
        path: 'dashboard',
        title: 'Dashboard',
        canMatch: [permissionGuard],
        data: { permissions: [Permissions.Dashboard.View] },
        loadComponent: () => import('@features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'board',
        title: 'Task board',
        canMatch: [permissionGuard],
        data: { permissions: [Permissions.Tasks.View] },
        loadComponent: () => import('@features/board/board').then((m) => m.Board),
      },
      {
        path: 'roles',
        title: 'Roles & permissions',
        canMatch: [permissionGuard],
        data: { permissions: [Permissions.Roles.View] },
        loadComponent: () => import('@features/roles/roles-page').then((m) => m.RolesPage),
      },
      {
        path: 'no-access',
        title: 'No access',
        component: StatusPage,
        data: {
          code: '403',
          heading: 'Nothing to show yet',
          message: 'Your role doesn’t include any pages. Ask an administrator to grant you access.',
          showHomeLink: false,
        },
      },
      { path: '', pathMatch: 'full', redirectTo: homePath },
      { path: '**', title: 'Not found', component: StatusPage },
    ],
  },
  { path: '**', redirectTo: '' },
];
