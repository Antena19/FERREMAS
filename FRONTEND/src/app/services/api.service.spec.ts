import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:7091/api'; // puerto backend

  constructor(private http: HttpClient) {}

  // 🔐 Login
  login(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, data);
  }

  // 📦 Productos
  getProductos(): Observable<any> {
    return this.http.get(`${this.baseUrl}/productos`);
  }

  getProducto(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/productos/${id}`);
  }

  // 👤 Clientes
  getClientes(): Observable<any> {
    return this.http.get(`${this.baseUrl}/clientes`);
  }

  getCliente(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/clientes/${id}`);
  }

  // 🛒 Carrito
  getCarrito(usuarioId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/carrito/${usuarioId}`);
  }

  // 📑 Pedidos
  getPedidos(): Observable<any> {
    return this.http.get(`${this.baseUrl}/pedidos`);
  }

  getPedido(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/pedidos/${id}`);
  }

  crearPedido(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/pedidos`, data);
  }
}
