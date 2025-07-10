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
  getProductos(todos: boolean = false): Observable<any[]> {
    const url = todos ? `${this.baseUrl}/productos?todos=true` : `${this.baseUrl}/productos`;
    return this.http.get<any[]>(url);
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

  /** ✅ Obtener todos los pedidos (para administradores) */
  getTodosLosPedidos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pedidos`);
  }

  /** ✅ Obtener historial de compras de un cliente específico */
  getHistorialComprasCliente(clienteId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pedidos/cliente/${clienteId}/historial`);
  }

  /** Crear un pedido con datos completos */
  crearPedido(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pedidos`, data);
  }

  /** Crear un pago asociado a un pedido */
  crearPago(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pagos`, data);
  }

  /** Crear transacción Webpay */
  crearTransaccionWebpay(data: { amount: number, pedidoId: number }): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pagos/webpay/crear`, data);
  }

  /** Confirmar transacción Webpay */
  confirmarTransaccionWebpay(token: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pagos/webpay/confirmar`, { token });
  }

  /** Crear o actualizar el pedido pendiente del usuario autenticado a partir del carrito */
  crearOActualizarPedidoDesdeCarrito(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/pedidos/desde-carrito`, data);
  }

  /** Obtener el pedido pendiente del usuario autenticado */
  getPedidoPendiente(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/pedidos/pendiente`);
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

  /** ✅ Obtener todas las categorías */
  getCategorias(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/categorias`);
  }

  /** ✅ Obtener todas las sucursales */
  getSucursales(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/admin/sucursales`);
  }

  /** Obtener inventario de un producto en una sucursal */
  getInventarioPorProductoYSucursal(productoId: number, sucursalId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/inventario/producto/${productoId}/sucursal/${sucursalId}`);
  }

  /** ✅ Obtener todas las marcas */
  getMarcas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/marcas`);
  }

  /** ✅ Actualizar producto */
  actualizarProducto(id: number, producto: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/productos/${id}`, producto);
  }

  /** Subir imagen de producto */
  subirImagenProducto(idProducto: number, formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/productos/${idProducto}/subir-imagen`, formData);
  }

  /** Carga masiva de productos desde archivo CSV */
  cargarProductosCsv(archivo: File): Observable<any> {
    const formData = new FormData();
    formData.append('archivoCsv', archivo);
    return this.http.post<any>(`${this.baseUrl}/productos/carga-masiva`, formData);
  }

  /** Crear producto */
  crearProducto(producto: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/productos`, producto);
  }

  /** Obtener sucursales activas (público) */
  getSucursalesActivas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/admin/sucursales/activas`);
  }

  /** Obtener inventario completo de una sucursal */
  getInventarioPorSucursal(sucursalId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/inventario/sucursal/${sucursalId}`);
  }

  /** Actualizar stock de un inventario por ID */
  actualizarStockInventario(inventarioId: number, cantidad: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/inventario/${inventarioId}/stock`, cantidad);
  }

  /** Obtener inventario global de todos los productos en todas las sucursales */
  getInventarioGlobal(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/bodeguero/inventario`);
  }

  /** Actualizar inventario completo (stock y stock mínimo) */
  actualizarInventarioCompleto(inventario: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/bodeguero/inventario`, inventario);
  }

  /** Eliminar producto por ID */
  eliminarProducto(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/productos/${id}`);
  }

}
