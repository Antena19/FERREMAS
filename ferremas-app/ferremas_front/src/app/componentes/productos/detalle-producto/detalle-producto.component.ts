import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductoService } from '../../../servicios/producto.service';
import { ProductoDetalle } from '../../../modelos/producto.model';
import { CarritoService } from '../../../servicios/carrito.service';
import { NotificacionService } from '../../../servicios/notificacion.service';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-detalle-producto',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './detalle-producto.component.html',
  styleUrls: ['./detalle-producto.component.css']
})
export class DetalleProductoComponent implements OnInit {
  producto: ProductoDetalle | null = null;
  cantidad: number = 1;
  cargando: boolean = true;
  error: string = '';
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productoService: ProductoService,
    private carritoService: CarritoService,
    private notificacionService: NotificacionService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.cargarProducto(+id);
    }
  }

  cargarProducto(id: number): void {
    this.cargando = true;
    this.productoService.obtenerPorId(id).subscribe({
      next: (producto: ProductoDetalle) => {
        this.producto = producto;
        this.cargando = false;
      },
      error: (error: Error) => {
        this.error = 'Error al cargar el producto';
        console.error('Error:', error);
        this.cargando = false;
      }
    });
  }

  agregarAlCarrito(): void {
    if (this.cantidad > 0 && this.producto) {
      this.carritoService.agregarProducto(this.producto.id, this.cantidad, this.producto)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.notificacionService.exito('Producto agregado al carrito');
          },
          error: (error) => {
            this.notificacionService.error('Error al agregar el producto al carrito');
            console.error('Error:', error);
          }
        });
    }
  }

  cambiarCantidad(cambio: number): void {
    const nuevaCantidad = this.cantidad + cambio;
    if (nuevaCantidad > 0) {
      this.cantidad = nuevaCantidad;
    }
  }

  volverAlCatalogo(): void {
    this.router.navigate(['/publico/catalogo']);
  }

  verCarrito() {
    this.router.navigate(['/carrito']);
  }

  checkout() {
    this.router.navigate(['/checkout']);
  }
} 