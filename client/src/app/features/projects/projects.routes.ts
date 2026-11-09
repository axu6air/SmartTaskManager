import { Routes } from '@angular/router';

export const PROJECT_ROUTES: Routes = [
  {
    path: '',
    title: 'Projects',
    loadComponent: () => import('./pages/project-list').then((m) => m.ProjectList),
  },
  {
    path: ':projectId',
    title: 'Project',
    loadComponent: () => import('./pages/project-detail').then((m) => m.ProjectDetail),
  },
];
