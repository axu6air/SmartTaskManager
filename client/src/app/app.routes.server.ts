import { RenderMode, ServerRoute } from '@angular/ssr';

// Every page sits behind sign-in and reads tokens from localStorage, so nothing can be prerendered.
export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    renderMode: RenderMode.Client,
  },
];
