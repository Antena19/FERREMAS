import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CarritoComponent } from '../../componentes/carrito/carrito.component';

const routes: Routes = [
  { path: '', component: CarritoComponent }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    CarritoComponent
  ]
})
export class CarritoModule { } 