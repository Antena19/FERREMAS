import { Routes } from '@angular/router';

export const publicoRoutes: Routes = [
  { 
    path: 'catalogo', 
    loadComponent: () => import('./catalogo/lista-productos/lista-productos.component').then(m => m.ListaProductosComponent)
  },
  { 
    path: 'catalogo/producto/:id', 
    loadComponent: () => import('./catalogo/detalle-producto/detalle-producto.component').then(m => m.DetalleProductoComponent)
  },
  {
    path: 'autenticacion/login',
    loadComponent: () => import('./autenticacion/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'autenticacion/registro',
    loadComponent: () => import('./autenticacion/registro/registro.component').then(m => m.RegistroComponent)
  },
  { 
    path: '', 
    redirectTo: 'catalogo', 
    pathMatch: 'full' 
  }
];
