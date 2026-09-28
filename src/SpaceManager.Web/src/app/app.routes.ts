import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Public
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'services',
    loadComponent: () =>
      import('./features/services/services-list/services-list').then((m) => m.ServicesList),
  },
  {
    path: 'professionals',
    loadComponent: () =>
      import('./features/professionals/professionals-list/professionals-list').then(
        (m) => m.ProfessionalsList,
      ),
  },
  // Protected (authGuard → /login when there is no JWT)
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'reservations',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/reservations/reservations-list/reservations-list').then(
        (m) => m.ReservationsList,
      ),
  },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', pathMatch: 'full', redirectTo: 'dashboard' },
];
