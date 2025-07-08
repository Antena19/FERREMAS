import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class CarritoService {
  private readonly STORAGE_KEY = 'carrito_local';
  private carritoSubject = new BehaviorSubject<any[]>([]);

  constructor(private api: ApiService, private auth: AuthService) {
    this.cargarCarritoInicial();
    // Escuchar cambios de login/logout para sincronizar
    this.auth.getAuthState().subscribe(usuario => {
      if (usuario && usuario.id) {
        this.sincronizarAlLogin();
      } else {
        // Si se desloguea, cargar carrito local
        this.cargarCarritoLocal();
      }
    });
  }

  /**
   * Carga el carrito inicial según el estado de autenticación
   */
  private cargarCarritoInicial() {
    if (this.auth.estaAutenticado()) {
      this.cargarCarritoBackend();
    } else {
      this.cargarCarritoLocal();
    }
  }

  /**
   * Devuelve observable del carrito actual
   */
  getCarrito$(): Observable<any[]> {
    return this.carritoSubject.asObservable();
  }

  /**
   * Agrega un producto al carrito (local o backend)
   */
  agregarProducto(producto: any, cantidad: number = 1): Observable<any> {
    if (this.auth.estaAutenticado()) {
      const usuarioId = this.auth.obtenerIdUsuario();
      return this.api.agregarAlCarrito({ usuarioId, productoId: producto.id, cantidad }).pipe(
        tap(() => this.cargarCarritoBackend())
      );
    } else {
      // Carrito local
      let carrito = this.obtenerCarritoLocal();
      const idx = carrito.findIndex((p: any) => p.id === producto.id);
      if (idx > -1) {
        carrito[idx].cantidad += cantidad;
      } else {
        carrito.push({ ...producto, cantidad });
      }
      this.guardarCarritoLocal(carrito);
      this.carritoSubject.next(carrito);
      return of(carrito);
    }
  }

  /**
   * Elimina un producto del carrito
   */
  eliminarProducto(productoId: number): Observable<any> {
    if (this.auth.estaAutenticado()) {
      const usuarioId = this.auth.obtenerIdUsuario();
      if (usuarioId === null) return of([]);
      return this.api.eliminarDelCarrito(usuarioId, productoId).pipe(
        tap(() => this.cargarCarritoBackend())
      );
    } else {
      let carrito = this.obtenerCarritoLocal().filter((p: any) => p.id !== productoId);
      this.guardarCarritoLocal(carrito);
      this.carritoSubject.next(carrito);
      return of(carrito);
    }
  }

  /**
   * Vacía el carrito
   */
  vaciarCarrito(): Observable<any> {
    if (this.auth.estaAutenticado()) {
      const usuarioId = this.auth.obtenerIdUsuario();
      if (usuarioId === null) return of([]);
      return this.api.vaciarCarrito(usuarioId).pipe(
        tap(() => this.cargarCarritoBackend())
      );
    } else {
      this.guardarCarritoLocal([]);
      this.carritoSubject.next([]);
      return of([]);
    }
  }

  /**
   * Carga el carrito desde el backend
   */
  private cargarCarritoBackend() {
    const usuarioId = this.auth.obtenerIdUsuario();
    if (!usuarioId) return;
    this.api.getCarritoPorUsuario(usuarioId).subscribe({
      next: (res: any) => {
        // Adaptar según estructura de respuesta
        let items = res.items || res.productos || res.detalle || res || [];
        // Si los items tienen producto anidado, aplanar
        if (items.length && items[0].producto) {
          items = items.map((i: any) => ({ ...i.producto, cantidad: i.cantidad }));
        }
        this.carritoSubject.next(items);
      },
      error: () => {
        this.carritoSubject.next([]);
      }
    });
  }

  /**
   * Carga el carrito local
   */
  private cargarCarritoLocal() {
    const carrito = this.obtenerCarritoLocal();
    this.carritoSubject.next(carrito);
  }

  /**
   * Obtiene el carrito local desde localStorage
   */
  private obtenerCarritoLocal(): any[] {
    const data = localStorage.getItem(this.STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  }

  /**
   * Guarda el carrito local en localStorage
   */
  private guardarCarritoLocal(carrito: any[]) {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(carrito));
  }

  /**
   * Sincroniza el carrito local con el backend al loguearse
   */
  sincronizarAlLogin() {
    const carritoLocal = this.obtenerCarritoLocal();
    if (!carritoLocal.length) {
      this.cargarCarritoBackend();
      return;
    }
    const usuarioId = this.auth.obtenerIdUsuario();
    if (!usuarioId) return;
    // Adaptar al formato esperado por el backend
    const items = carritoLocal.map(p => ({ productoId: p.id, cantidad: p.cantidad }));
    this.api.sincronizarCarrito({ usuarioId, items }).subscribe({
      next: () => {
        this.guardarCarritoLocal([]);
        this.cargarCarritoBackend();
      },
      error: () => {
        // Si falla, mantener el carrito local
      }
    });
  }
} 