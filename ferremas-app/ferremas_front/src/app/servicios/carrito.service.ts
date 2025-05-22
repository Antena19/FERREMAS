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
    private router: Router
  ) {
    this.cargarCarrito();
  }

  private cargarCarrito(): void {
    if (this.authService.isAuthenticated()) {
      const usuarioId = this.authService.getUsuarioId();
      if (usuarioId) {
        this.http.get<Carrito>(`${this.apiUrl}/${usuarioId}`)
          .pipe(
            tap(carrito => this.carritoSubject.next(carrito)),
            catchError(error => {
              this.notificacionService.error('Error al cargar el carrito');
              return of(this.obtenerCarritoLocal());
            })
          ).subscribe();
      } else {
        this.carritoSubject.next(this.obtenerCarritoLocal());
      }
    } else {
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
      return this.http.delete<Carrito>(`${this.apiUrl}/eliminar-item`, { body: { usuarioId, productoId } }).pipe(
        tap(carrito => this.carritoSubject.next(carrito)),
        catchError(error => {
          this.notificacionService.error('Error al eliminar el producto del carrito');
          throw error;
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
      return this.http.get<Carrito>(`${this.apiUrl}`).pipe(
        tap(carrito => this.carritoSubject.next(carrito)),
        catchError(error => {
          this.notificacionService.error('Error al cargar el carrito');
          return of(this.obtenerCarritoLocal());
        })
      );
    }
    return this.carrito$;
  }

  sincronizarCarritoConBackend(): Observable<any> {
    const carritoLocal = this.obtenerCarritoLocal();
    const usuarioId = this.authService.getUsuarioId();

    if (usuarioId && carritoLocal && carritoLocal.items.length > 0) {
      const peticiones = carritoLocal.items.map(item =>
        this.http.post(`${this.apiUrl}/agregar`, {
          usuarioId,
          productoId: item.productoId,
          cantidad: item.cantidad
        })
      );
      return forkJoin(peticiones).pipe(
        switchMap(() => {
          // Una vez agregados todos, obtenemos el carrito actualizado
          return this.http.get<Carrito>(`${this.apiUrl}/${usuarioId}`).pipe(
            tap(carrito => {
              this.carritoSubject.next(carrito);
              localStorage.removeItem(this.LOCAL_KEY);
            })
          );
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