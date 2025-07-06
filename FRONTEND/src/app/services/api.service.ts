import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  // ✅ Dirección base del backend API REST
  private baseUrl = 'https://localhost:7091/api';

  constructor(private http: HttpClient) {}

  // ============================
  // 📦 MÓDULO HOME
  // ============================

  /** ✅ Obtener los productos más recientes (home) */
  getProductosRecientes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/productos`);
  }

  // ============================
  // 📦 MÓDULO PRODUCTOS
  // ============================

  /** ✅ Obtener todos los productos */
  getProductos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/productos`);
  }

  /** ✅ Obtener un producto por ID */
  getProductoPorId(id: number | string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/productos/${id}`);
  }

  // ============================
  // 🛒 MÓDULO CARRITO
  // ============================

  /** ✅ Agregar un producto al carrito */
  agregarAlCarrito(item: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/carrito/agregar`, item);
  }

  /** ✅ Obtener carrito de un usuario */
  getCarritoPorUsuario(idUsuario: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/carrito/${idUsuario}`);
  }

  /** ✅ Actualizar cantidad de un producto en el carrito */
  actualizarCantidadCarrito(item: { idUsuario: number, idProducto: number, cantidad: number }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/carrito/actualizar-cantidad`, item);
  }

  /** ✅ Eliminar un producto del carrito */
  eliminarDelCarrito(idUsuario: number, idProducto: number): Observable<any> {
    return this.http.request<any>('delete', `${this.baseUrl}/carrito/eliminar-item`, {
      body: { idUsuario, idProducto }
    });
  }

  /** ✅ Vaciar completamente el carrito */
  vaciarCarrito(idUsuario: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/carrito/vaciar/${idUsuario}`);
  }

  /** ✅ Sincronizar carrito con backend */
  sincronizarCarrito(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/carrito/sincronizar`, data);
  }

  // ============================
  // 🧾 MÓDULO PEDIDOS
  // ============================

  /** ✅ Confirmar pedido del usuario */
  confirmarPedido(idUsuario: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pedido`, { idUsuario });
  }

  /** ✅ Obtener todos los pedidos del usuario */
  getPedidosPorUsuario(idUsuario: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pedido/usuario/${idUsuario}`);
  }

  /** ✅ Obtener el detalle de un pedido */
  getDetallePedido(idPedido: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/pedido/${idPedido}`);
  }

  /** ✅ Obtener historial de compras del usuario autenticado */
  getMiHistorialCompras(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pedidos/mi-historial`);
  }

  /** ✅ Obtener historial de compras de un cliente específico */
  getHistorialComprasCliente(clienteId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pedidos/cliente/${clienteId}/historial`);
  }

  // ============================
  // 👤 MÓDULO CLIENTES
  // ============================

  /** ✅ Obtener todos los clientes */
  getClientes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/clientes`);
  }

  /** ✅ Obtener cliente por ID */
  getClientePorId(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/clientes/${id}`);
  }

  /** ✅ Obtener perfil completo del usuario */
  getPerfilUsuario(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/admin/perfil/${id}`);
  }

  /** ✅ Obtener perfil del usuario autenticado */
  getMiPerfil(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/perfil/mi-perfil`);
  }

  /** ✅ Obtener direcciones del usuario autenticado */
  getMisDirecciones(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/perfil/direcciones`);
  }

  /** ✅ Agregar nueva dirección desde perfil */
  agregarDireccionPerfil(direccion: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/perfil/direcciones`, direccion);
  }

  /** ✅ Actualizar dirección existente desde perfil */
  actualizarDireccionPerfil(id: number, direccion: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfil/direcciones/${id}`, direccion);
  }

  /** ✅ Eliminar dirección desde perfil */
  eliminarDireccionPerfil(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/perfil/direcciones/${id}`);
  }

  /** ✅ Actualizar datos personales del usuario autenticado */
  actualizarDatosPersonales(datos: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfil/datos-personales`, datos);
  }

  /** ✅ Probar token de autenticación */
  testToken(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/perfil/test-token`);
  }

  crearCliente(cliente: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/registro`, cliente);
  }

  /** ✅ Actualizar cliente existente */
  actualizarCliente(id: number, cliente: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/clientes/${id}`, cliente);
  }

  /** ✅ Obtener direcciones de un cliente */
  getDireccionesCliente(idCliente: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/clientes/${idCliente}/direcciones`);
  }

  /** ✅ Crear nueva dirección */
  crearDireccion(idCliente: number, direccion: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/clientes/${idCliente}/direcciones`, direccion);
  }

  /** ✅ Actualizar dirección */
  actualizarDireccion(idDireccion: number, direccion: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/clientes/direcciones/${idDireccion}`, direccion);
  }

  /** ✅ Eliminar dirección */
  eliminarDireccion(idDireccion: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/clientes/direcciones/${idDireccion}`);
  }

  // ============================
  // 🔐 LOGIN
  // ============================

  /** ✅ Login con email y contraseña */
  login(credenciales: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/login`, credenciales);
  }

  /**
   * ✅ Enviar solicitud de recuperación de contraseña
   * POST /api/Auth/recuperar-contrasena
   * @param data Objeto con la propiedad Email
   * @returns Observable con el resultado de la solicitud
   */
  recuperarContrasena(data: { Email: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/Auth/recuperar-contrasena`, data);
  }

}
