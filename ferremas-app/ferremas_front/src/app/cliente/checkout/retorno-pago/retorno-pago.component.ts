import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PagoService } from '../../../servicios/pago.service';
import { CarritoService } from '../../../servicios/carrito.service';

@Component({
  selector: 'app-retorno-pago',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './retorno-pago.component.html',
  styleUrls: ['./retorno-pago.component.css']
})
export class RetornoPagoComponent implements OnInit {
  pagoExitoso = false;
  estadoPago = '';
  mensajeError = '';
  pedidoId?: string;
  cargando = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pagoService: PagoService,
    private carritoService: CarritoService
  ) {}

  ngOnInit(): void {
    // Obtener parámetros de la URL
    this.route.queryParams.subscribe(params => {
      this.pedidoId = params['external_reference'];
      const pagoId = params['payment_id'];
      const status = params['status'];
      
      if (status) {
        this.verificarEstadoPago(status, pagoId);
      } else {
        this.mensajeError = 'No se pudo obtener el estado del pago.';
        this.cargando = false;
      }
    });
  }

  verificarEstadoPago(status: string, pagoId?: string): void {
    switch (status) {
      case 'approved':
        this.pagoExitoso = true;
        this.estadoPago = 'aprobado';
        // Vaciar el carrito
        this.carritoService.vaciarCarrito();
        break;
      case 'pending':
        this.estadoPago = 'pendiente';
        this.mensajeError = 'El pago está pendiente de confirmación.';
        break;
      case 'in_process':
        this.estadoPago = 'en proceso';
        this.mensajeError = 'El pago está siendo procesado.';
        break;
      case 'rejected':
        this.estadoPago = 'rechazado';
        this.mensajeError = 'El pago fue rechazado. Por favor, intenta nuevamente con otro método de pago.';
        break;
      default:
        this.estadoPago = 'desconocido';
        this.mensajeError = 'Estado de pago desconocido.';
    }
    
    // Si tenemos el ID del pago, verificamos su estado en nuestra API
    if (pagoId) {
      this.pagoService.verificarPagoMercadoPago(pagoId).subscribe({
        next: (respuesta) => {
          // Asegurarse de que la respuesta coincide con el estado que recibimos
          if (respuesta.status !== status) {
            console.warn('Estado del pago inconsistente:', respuesta.status, status);
          }
          this.cargando = false;
        },
        error: (err) => {
          console.error('Error al verificar el pago', err);
          this.cargando = false;
        }
      });
    } else {
      this.cargando = false;
    }
  }
}