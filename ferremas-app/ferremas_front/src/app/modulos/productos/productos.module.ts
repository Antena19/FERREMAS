import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductosComponent } from '../../componentes/productos/productos.component';
import { DetalleProductoComponent } from '../../componentes/productos/detalle-producto/detalle-producto.component';

const routes: Routes = [
  { path: '', component: ProductosComponent },
  { path: ':id', component: DetalleProductoComponent }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    ProductosComponent,
    DetalleProductoComponent
  ]
})
export class ProductosModule { } 