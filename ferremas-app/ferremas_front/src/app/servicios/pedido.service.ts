import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Pedido, PedidoCreate, EstadoPedido } from '../modelos/pedido.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private apiUrl = `${environment.apiUrl}/pedidos`;

  constructor(private http: HttpClient) { }

  obtenerTodos(): Observable<Pedido[]> {
    return this.http.get<Pedido[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.apiUrl}/${id}`);
  }

  obtenerPedidosUsuario(usuarioId: number): Observable<Pedido[]> {
    return this.http.get<Pedido[]>(`${this.apiUrl}/usuario/${usuarioId}`);
  }

  obtenerPedidosPorEstado(estado: EstadoPedido): Observable<Pedido[]> {
    return this.http.get<Pedido[]>(`${this.apiUrl}/estado/${estado}`);
  }

  crearPedido(pedido: PedidoCreate): Observable<number> {
    return this.http.post<number>(this.apiUrl, pedido);
  }

  actualizarEstadoPedido(id: number, estado: EstadoPedido): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/estado`, { estado });
  }

  cancelarPedido(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/cancelar`, {});
  }
}