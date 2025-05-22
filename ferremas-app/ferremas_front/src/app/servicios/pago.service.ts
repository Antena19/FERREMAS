import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Pago, PagoCreate, RespuestaMercadoPago } from '../modelos/pago.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PagoService {
  private apiUrl = `${environment.apiUrl}/pagos`;

  constructor(private http: HttpClient) { }

  obtenerPorPedidoId(pedidoId: number): Observable<Pago> {
    return this.http.get<Pago>(`${this.apiUrl}/pedido/${pedidoId}`);
  }

  crearPago(pago: PagoCreate): Observable<number> {
    return this.http.post<number>(this.apiUrl, pago);
  }

  // Método para crear una preferencia de pago en Mercado Pago
  crearPreferenciaMercadoPago(pedidoId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/mercadopago/preferencia`, { pedidoId });
  }

  // Método para verificar el estado de un pago en Mercado Pago
  verificarPagoMercadoPago(pagoId: string): Observable<RespuestaMercadoPago> {
    return this.http.get<RespuestaMercadoPago>(`${this.apiUrl}/mercadopago/verificar/${pagoId}`);
  }

  // Método para manejar la notificación (webhook) de Mercado Pago
  procesarNotificacionMercadoPago(notificacion: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/mercadopago/notificacion`, notificacion);
  }
}