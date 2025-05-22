import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VistaCarritoComponent } from './carrito/vista-carrito/vista-carrito.component';
import { CheckoutPasosComponent } from './checkout/checkout-pasos/checkout-pasos.component';
import { RetornoPagoComponent } from './checkout/retorno-pago/retorno-pago.component';
import { AuthGuard } from '../nucleo/guardias/auth.guard';

const routes: Routes = [
  { 
    path: 'carrito', 
    component: VistaCarritoComponent 
  },
  { 
    path: 'checkout', 
    component: CheckoutPasosComponent,
    canActivate: [AuthGuard],
    data: { roles: ['cliente'] }
  },
  { 
    path: 'checkout/retorno', 
    component: RetornoPagoComponent 
  },
  { path: '**', redirectTo: 'carrito' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClienteRoutingModule { }