import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component'; // ✅ Importamos el layout

const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: '',
    component: LayoutComponent, // ✅ Aquí aplicamos el layout con footer
    children: [
      {
        path: 'home',
        loadComponent: () => import('./pages/home/home.page').then(m => m.HomePage)
      },
      {
        path: 'productos',
        loadComponent: () => import('./pages/productos/productos.page').then(m => m.ProductosPage)
      },
      {
        path: 'detalle-producto/:id',
        loadComponent: () => import('./pages/detalle-producto/detalle-producto.page').then(m => m.DetalleProductoPage)
      },
      {
        path: 'carrito',
        loadComponent: () => import('./pages/carrito/carrito.page').then(m => m.CarritoPage)
      },
      {
        path: 'clientes',
        loadComponent: () => import('./pages/clientes/clientes.page').then(m => m.ClientesPage)
      },
      {
        path: 'detalle-pedido/:id',
        loadComponent: () => import('./pages/detalle-pedido/detalle-pedido.page').then(m => m.DetallePedidoPage)
      },
      {
        path: 'pedidos',
        loadComponent: () => import('./pages/pedidos/pedidos.page').then(m => m.PedidosPage)
      },
      {
        path: 'editar-perfil',
        loadComponent: () => import('./pages/editar-perfil/editar-perfil.page').then( m => m.EditarPerfilPage)
      },
      {
        path: 'login',
        loadComponent: () => import('./pages/login/login.page').then(m => m.LoginPage)
      },
      {
        path: 'crear-cliente',
        loadComponent: () => import('./pages/crear-cliente/crear-cliente.page').then(m => m.CrearClientePage)
      },

      {
        path: 'cambiar-contrasena',
        loadComponent: () => import('./auth/cambiar-contrasena/cambiar-contrasena.page').then(m => m.CambiarContrasenaPage)
      },
      {
        path: 'usuarios',
        loadComponent: () => import('./admin/usuarios/usuarios.page').then(m => m.UsuariosPage)
      },
      {
        path: 'usuario-detalle',
        loadComponent: () => import('./admin/usuario-detalle/usuario-detalle.page').then(m => m.UsuarioDetallePage)
      },
      {
        path: 'vendedores',
        loadComponent: () => import('./admin/vendedores/vendedores.page').then(m => m.VendedoresPage)
      },
      {
        path: 'bodegueros',
        loadComponent: () => import('./admin/bodegueros/bodegueros.page').then(m => m.BodeguerosPage)
      },
      {
        path: 'contadores',
        loadComponent: () => import('./admin/contadores/contadores.page').then(m => m.ContadoresPage)
      },
      {
        path: 'listado-categorias',
        loadComponent: () => import('./categorias/listado/listado.page').then(m => m.ListadoPage)
      },
      {
        path: 'formulario-categorias',
        loadComponent: () => import('./categorias/formulario/formulario.page').then(m => m.FormularioPage)
      },
      {
        path: 'listado-marcas',
        loadComponent: () => import('./marcas/listado/listado.page').then(m => m.ListadoPage)
      },
      {
        path: 'confirmacion-carrito',
        loadComponent: () => import('./carrito/confirmacion/confirmacion.page').then(m => m.ConfirmacionPage)
      },
      {
        path: 'crear-pedido',
        loadComponent: () => import('./pedidos/crear/crear.page').then(m => m.CrearPage)
      },
      {
        path: 'inventario',
        loadComponent: () => import('./bodeguero/inventario/inventario.page').then(m => m.InventarioPage)
      },
      {
        path: 'pedidos-bodega',
        loadComponent: () => import('./bodeguero/pedidos/pedidos.page').then(m => m.PedidosPage)
      },
      {
        path: 'pedido-detalle-bodega',
        loadComponent: () => import('./bodeguero/pedido-detalle/pedido-detalle.page').then(m => m.PedidoDetallePage)
      },
      {
        path: 'actualizar-producto',
        loadComponent: () => import('./bodeguero/actualizar-producto/actualizar-producto.page').then(m => m.ActualizarProductoPage)
      },
      {
        path: 'pagos-pendientes',
        loadComponent: () => import('./contador/pagos-pendientes/pagos-pendientes.page').then(m => m.PagosPendientesPage)
      },
      {
        path: 'historial-pagos',
        loadComponent: () => import('./contador/historial/historial.page').then(m => m.HistorialPage)
      },
      {
        path: 'detalle-pago',
        loadComponent: () => import('./contador/detalle-pago/detalle-pago.page').then(m => m.DetallePagoPage)
      },
      {
        path: 'transferencia',
        loadComponent: () => import('./pagos/transferencia/transferencia.page').then(m => m.TransferenciaPage)
      },
      {
        path: 'confirmacion-pago',
        loadComponent: () => import('./pagos/confirmacion/confirmacion.page').then(m => m.ConfirmacionPage)
      },
      {
        path: 'clientes-vendedor',
        loadComponent: () => import('./vendedor/clientes/clientes.page').then(m => m.ClientesPage)
      },
      {
        path: 'pedidos-vendedor',
        loadComponent: () => import('./vendedor/pedidos/pedidos.page').then(m => m.PedidosPage)
      },
      {
        path: 'pedido-detalle-vendedor',
        loadComponent: () => import('./vendedor/pedido-detalle/pedido-detalle.page').then(m => m.PedidoDetallePage)
      },
      {
        path: 'crear-pedido-bodega',
        loadComponent: () => import('./vendedor/crear-pedido-bodega/crear-pedido-bodega.page').then(m => m.CrearPedidoBodegaPage)
      },
      {
        path: 'recuperar-contrasena',
        loadComponent: () => import('./pages/recuperar-contrasena/recuperar-contrasena.page').then(m => m.RecuperarContrasenaPage)
      },
      { path: '', redirectTo: 'home', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
