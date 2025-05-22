import { Routes } from '@angular/router';
import { AuthGuard } from './nucleo/guardias/auth.guard';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Router } from '@angular/router';
import { publicoRoutes } from './publico/publico.routes';

export const routes: Routes = [
  ...publicoRoutes,
  {
    path: 'autenticacion/login',
    loadComponent: () => import('./publico/autenticacion/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'autenticacion/registro',
    loadComponent: () => import('./publico/autenticacion/registro/registro.component').then(m => m.RegistroComponent)
  },
  {
    path: 'cliente',
    loadChildren: () => import('./cliente/cliente-routing.module').then(m => m.ClienteRoutingModule)
  },
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin-routing.module').then(m => m.AdminRoutingModule),
    canActivate: [AuthGuard],
    data: { roles: ['administrador'] }
  },
  {
    path: 'vendedor',
    loadChildren: () => import('./vendedor/vendedor-routing.module').then(m => m.VendedorRoutingModule),
    canActivate: [AuthGuard],
    data: { roles: ['vendedor'] }
  },
  {
    path: 'bodeguero',
    loadChildren: () => import('./bodeguero/bodeguero-routing.module').then(m => m.BodegueroRoutingModule),
    canActivate: [AuthGuard],
    data: { roles: ['bodeguero'] }
  },
  {
    path: 'contador',
    loadChildren: () => import('./contador/contador-routing.module').then(m => m.ContadorRoutingModule),
    canActivate: [AuthGuard],
    data: { roles: ['contador'] }
  },
  {
    path: '**',
    redirectTo: ''
  }
];
