import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, forkJoin } from 'rxjs';
import { map, tap, catchError, switchMap } from 'rxjs/operators';
import { Carrito, ItemCarrito } from '../modelos/carrito.model';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { NotificacionService } from './notificacion.service';
import { Producto } from '../modelos/producto.model';
import { Router } from '@angular/router';
import { EventosService } from './eventos.service';

@Injectable({
  providedIn: 'root'
})
export class CarritoService {
  private apiUrl = `${environment.apiUrl}/carrito`;
  
  private carritoSubject = new BehaviorSubject<Carrito>({
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  });
  
  public carrito$ = this.carritoSubject.asObservable();

  private LOCAL_KEY = 'carrito_local';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private notificacionService: NotificacionService,
    private router: Router,
    private eventosService: EventosService
  ) {
    // Suscribirse al evento de login
    this.eventosService.login$.subscribe(() => {
      console.log('Evento de login recibido en CarritoService');
      this.cargarCarrito();
    });

    // Cargar carrito inicial
    this.cargarCarrito();
  }

  private cargarCarrito(): void {
    console.log('Cargando carrito...');
    if (this.authService.isAuthenticated()) {
      const usuarioId = this.authService.getUsuarioId();
      console.log('Usuario autenticado, ID:', usuarioId);
      
      if (usuarioId) {
        // Primero obtener el carrito del backend
        this.http.get<Carrito>(`${this.apiUrl}/${usuarioId}`)
          .pipe(
            tap(carritoBackend => {
              console.log('Carrito obtenido del backend:', carritoBackend);
            }),
            switchMap(carritoBackend => {
              // Si hay carrito en el backend, usarlo
              if (carritoBackend && carritoBackend.items.length > 0) {
                console.log('Usando carrito del backend');
                return of(carritoBackend);
              }
              
              // Si no hay carrito en el backend, verificar si hay carrito local
              const carritoLocal = this.obtenerCarritoLocal();
              console.log('Carrito local:', carritoLocal);
              
              if (carritoLocal && carritoLocal.items.length > 0) {
                console.log('Sincronizando carrito local con backend');
                // Sincronizar el carrito local con el backend
                const items = carritoLocal.items.map(item => ({
                  productoId: item.productoId,
                  cantidad: item.cantidad
                }));
                return this.http.post<Carrito>(`${this.apiUrl}/sincronizar`, {
                  usuarioId,
                  items
                });
              }
              
              console.log('No hay carrito en backend ni local, retornando carrito vacío');
              // Si no hay carrito ni en backend ni local, retornar carrito vacío
              return of({ items: [], subtotal: 0, impuestos: 0, descuentos: 0, total: 0 });
            }),
            tap(carrito => {
              console.log('Carrito final a mostrar:', carrito);
              this.carritoSubject.next(carrito);
              // Limpiar el carrito local después de sincronizar
              localStorage.removeItem(this.LOCAL_KEY);
            }),
            catchError(error => {
              console.error('Error al cargar/sincronizar el carrito:', error);
              this.notificacionService.error('Error al cargar el carrito');
              return of(this.obtenerCarritoLocal());
            })
          ).subscribe();
      } else {
        console.log('No hay ID de usuario, usando carrito local');
        this.carritoSubject.next(this.obtenerCarritoLocal());
      }
    } else {
      console.log('Usuario no autenticado, usando carrito local');
      this.carritoSubject.next(this.obtenerCarritoLocal());
    }
  }

  private calcularTotales(carrito: Carrito): Carrito {
    carrito.subtotal = carrito.items.reduce((sum, item) => sum + (item.precioUnitario * item.cantidad), 0);
    carrito.impuestos = carrito.subtotal * 0.19;
    carrito.total = carrito.subtotal + carrito.impuestos;
    return carrito;
  }

  private guardarCarritoLocal(carrito: Carrito) {
    const carritoConTotales = this.calcularTotales(carrito);
    console.log('Guardando carrito local:', carritoConTotales);
    localStorage.setItem(this.LOCAL_KEY, JSON.stringify(carritoConTotales));
    this.carritoSubject.next(carritoConTotales);
  }

  obtenerCarritoLocal(): Carrito {
    const data = localStorage.getItem(this.LOCAL_KEY);
    if (data) {
      return JSON.parse(data);
    }
    return { items: [], subtotal: 0, impuestos: 0, descuentos: 0, total: 0 };
  }

  agregarProducto(productoId: number, cantidad: number, producto?: Producto): Observable<Carrito> {
    if (!this.authService.isAuthenticated()) {
      let carrito = this.obtenerCarritoLocal();
      let item = carrito.items.find(i => i.productoId === productoId);
      
      if (item) {
        item.cantidad += cantidad;
        if (producto?.precio) {
          item.precioUnitario = producto.precio;
        }
        item.subtotal = item.precioUnitario * item.cantidad;
        console.log("Aumentando cantidad, actualizando precio y subtotal:", productoId, "precio:", item.precioUnitario, "cantidad:", item.cantidad, "subtotal:", item.subtotal);
      } else {
        console.log("Agregando producto:", productoId, "precio:", producto?.precio, "cantidad:", cantidad);
        carrito.items.push({
          productoId,
          cantidad,
          precioUnitario: producto?.precio ?? 0,
          subtotal: (producto?.precio ?? 0) * cantidad,
          producto: producto
        });
      }
      
      this.guardarCarritoLocal(carrito);
      return of(carrito);
    } else {
      const usuarioId = this.authService.getUsuarioId();
      console.log('Agregando al carrito backend:', { usuarioId, productoId, cantidad });
      return this.http.post<Carrito>(`${this.apiUrl}/agregar`, { usuarioId, productoId, cantidad }).pipe(
        tap(carrito => this.carritoSubject.next(carrito)),
        catchError(error => {
          this.notificacionService.error('Error al agregar el producto al carrito');
          throw error;
        })
      );
    }
  }

  actualizarCantidad(productoId: number, cantidad: number): Observable<Carrito> {
    if (!this.authService.isAuthenticated()) {
      let carrito = this.obtenerCarritoLocal();
      let item = carrito.items.find(i => i.productoId === productoId);
      
      if (item) {
        if (cantidad <= 0) {
          carrito.items = carrito.items.filter(i => i.productoId !== productoId);
        } else {
          item.cantidad = cantidad;
        }
        this.guardarCarritoLocal(carrito);
      }
      return of(carrito);
    } else {
      const usuarioId = this.authService.getUsuarioId();
      return this.http.put<Carrito>(`${this.apiUrl}/actualizar-cantidad`, { usuarioId, productoId, cantidad }).pipe(
        tap(carrito => this.carritoSubject.next(carrito)),
        catchError(error => {
          this.notificacionService.error('Error al actualizar la cantidad');
          throw error;
        })
      );
    }
  }

  eliminarProducto(productoId: number): Observable<Carrito> {
    if (!this.authService.isAuthenticated()) {
      let carrito = this.obtenerCarritoLocal();
      carrito.items = carrito.items.filter(i => i.productoId !== productoId);
      this.guardarCarritoLocal(carrito);
      return of(carrito);
    } else {
      const usuarioId = this.authService.getUsuarioId();
      return this.obtenerCarrito().pipe(
        switchMap(carrito => {
          const item = carrito.items.find(i => i.productoId === productoId);
          if (!item) {
            throw new Error('Producto no encontrado en el carrito');
          }
          return this.http.delete<Carrito>(`${this.apiUrl}/eliminar-item`, { 
            body: { usuarioId, itemId: item.id } 
          }).pipe(
            tap(carrito => this.carritoSubject.next(carrito)),
            catchError(error => {
              this.notificacionService.error('Error al eliminar el producto del carrito');
              throw error;
            })
          );
        })
      );
    }
  }

  vaciarCarrito(): Observable<void> {
    if (!this.authService.isAuthenticated()) {
      localStorage.removeItem(this.LOCAL_KEY);
      const carritoVacio = { items: [], subtotal: 0, impuestos: 0, descuentos: 0, total: 0 };
      this.carritoSubject.next(carritoVacio);
      return of();
    } else {
      const usuarioId = this.authService.getUsuarioId();
      return this.http.delete<void>(`${this.apiUrl}/vaciar/${usuarioId}`).pipe(
        tap(() => {
          const carritoVacio = { items: [], subtotal: 0, impuestos: 0, descuentos: 0, total: 0 };
          this.carritoSubject.next(carritoVacio);
        }),
        catchError(error => {
          this.notificacionService.error('Error al vaciar el carrito');
          throw error;
        })
      );
    }
  }

  obtenerCarrito(): Observable<Carrito> {
    if (this.authService.isAuthenticated()) {
      const usuarioId = this.authService.getUsuarioId();
      if (!usuarioId) {
        return of(this.obtenerCarritoLocal());
      }
      
      return this.http.get<Carrito>(`${this.apiUrl}/${usuarioId}`).pipe(
        tap(carrito => {
          console.log('Carrito obtenido:', carrito);
          this.carritoSubject.next(carrito);
        }),
        catchError(error => {
          console.error('Error al obtener el carrito:', error);
          this.notificacionService.error('Error al cargar el carrito');
          return of(this.obtenerCarritoLocal());
        })
      );
    }
    return of(this.obtenerCarritoLocal());
  }

  sincronizarCarritoConBackend(): Observable<any> {
    const carritoLocal = this.obtenerCarritoLocal();
    const usuarioId = this.authService.getUsuarioId();

    if (usuarioId && carritoLocal && carritoLocal.items.length > 0) {
      const items = carritoLocal.items.map(item => ({
        productoId: item.productoId,
        cantidad: item.cantidad
      }));
      return this.http.post(`${this.apiUrl}/sincronizar`, {
        usuarioId,
        items
      }).pipe(
        tap((carrito: any) => {
          this.carritoSubject.next(carrito);
          localStorage.removeItem(this.LOCAL_KEY);
        })
      );
    }
    return of(null);
  }

  verCarrito() {
    this.router.navigate(['/cliente/carrito']);
  }

  checkout() {
    this.router.navigate(['/cliente/checkout']);
  }
}