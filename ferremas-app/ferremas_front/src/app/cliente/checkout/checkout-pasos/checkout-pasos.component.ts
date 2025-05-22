import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CarritoService } from '../../../servicios/carrito.service';
import { AuthService } from '../../../servicios/auth.service';
import { PedidoService } from '../../../servicios/pedido.service';
import { PagoService } from '../../../servicios/pago.service';
import { Carrito } from '../../../modelos/carrito.model';
import { Usuario } from '../../../modelos/usuario.model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MetodoPago } from '../../../modelos/pago.model';
import { TipoEntrega } from '../../../modelos/pedido.model';
import { Subject, takeUntil } from 'rxjs';
import { Router } from '@angular/router';
import { NotificacionService } from '../../../servicios/notificacion.service';

@Component({
  selector: 'app-checkout-pasos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './checkout-pasos.component.html',
  styleUrls: ['./checkout-pasos.component.css']
})
export class CheckoutPasosComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  pasoActual = 1;
  carrito: Carrito = {
    items: [],
    subtotal: 0,
    impuestos: 0,
    descuentos: 0,
    total: 0
  };
  usuario?: Usuario;
  pedidoCreado = false;
  pedidoId?: number;
  isLoading = false;
  errorMessage = '';
  
  // Formularios
  datosClienteForm!: FormGroup;
  datosEnvioForm!: FormGroup;
  datosPagoForm!: FormGroup;
  
  // Opciones
  tiposEntrega = [
    { id: 'retiro_tienda' as TipoEntrega, nombre: 'Retiro en tienda' },
    { id: 'despacho_domicilio' as TipoEntrega, nombre: 'Despacho a domicilio' }
  ];
  
  metodosPago = [
    { id: 'mercadopago', nombre: 'Mercado Pago' },
    { id: 'transferencia', nombre: 'Transferencia bancaria' }
  ];
  
  sucursales = [
    { id: 1, nombre: 'FERREMAS Santiago Centro', direccion: 'Alameda 1234, Santiago' },
    { id: 2, nombre: 'FERREMAS Providencia', direccion: 'Providencia 567, Providencia' },
    { id: 3, nombre: 'FERREMAS Las Condes', direccion: 'Apoquindo 9876, Las Condes' },
    { id: 4, nombre: 'FERREMAS Maipú', direccion: 'Pajaritos 4321, Maipú' },
    { id: 5, nombre: 'FERREMAS Concepción', direccion: 'O\'Higgins 123, Concepción' },
    { id: 6, nombre: 'FERREMAS Viña del Mar', direccion: 'Valparaíso 456, Viña del Mar' },
    { id: 7, nombre: 'FERREMAS Puerto Montt', direccion: 'Diego Portales 789, Puerto Montt' }
  ];
  
  constructor(
    private carritoService: CarritoService,
    private authService: AuthService,
    private pedidoService: PedidoService,
    private pagoService: PagoService,
    private fb: FormBuilder,
    private router: Router,
    private notificacionService: NotificacionService
  ) {
    this.inicializarFormularios();
  }

  private inicializarFormularios(): void {
    this.datosClienteForm = this.fb.group({
      nombre: ['', [Validators.required, Validators.minLength(2)]],
      apellido: ['', [Validators.required, Validators.minLength(2)]],
      telefono: ['', [Validators.required, Validators.pattern(/^[0-9]{9}$/)]],
      correo: ['', [Validators.required, Validators.email]]
    });
    
    this.datosEnvioForm = this.fb.group({
      tipoEntrega: ['retiro_tienda' as TipoEntrega, Validators.required],
      sucursalId: [1, Validators.required],
      calle: [''],
      numero: [''],
      departamento: [''],
      comuna: [''],
      region: [''],
      codigoPostal: [''],
      notas: ['']
    });
    
    this.datosPagoForm = this.fb.group({
      metodoPago: ['mercadopago', Validators.required]
    });
    
    this.configurarValidadoresDinamicos();
  }

  private configurarValidadoresDinamicos(): void {
    this.datosEnvioForm.get('tipoEntrega')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(tipo => {
        const controles = ['calle', 'numero', 'comuna', 'region'];
        controles.forEach(control => {
          const formControl = this.datosEnvioForm.get(control);
          if (tipo === 'despacho_domicilio') {
            formControl?.setValidators([Validators.required]);
          } else {
            formControl?.clearValidators();
          }
          formControl?.updateValueAndValidity();
        });
      });
  }

  ngOnInit(): void {
    this.cargarDatosIniciales();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private cargarDatosIniciales(): void {
    this.carritoService.carrito$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(carrito => {
      this.carrito = carrito;
    });
    
    this.authService.usuario$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(usuario => {
      if (usuario) {
        this.usuario = usuario;
        this.datosClienteForm.patchValue({
          nombre: usuario.nombre,
          apellido: usuario.apellido,
          telefono: usuario.telefono,
          correo: usuario.email
        });
      }
    });
  }

  irAlPaso(paso: number): void {
    if (!this.validarPasoActual()) {
      return;
    }
    this.pasoActual = paso;
    this.errorMessage = '';
  }

  private validarPasoActual(): boolean {
    let formularioValido = true;
    
    switch (this.pasoActual) {
      case 1:
        formularioValido = this.datosClienteForm.valid;
        break;
      case 2:
        formularioValido = this.datosEnvioForm.valid;
        break;
      case 3:
        formularioValido = this.datosPagoForm.valid;
        break;
    }
    
    if (!formularioValido) {
      this.marcarTodosComoTocados(this.datosClienteForm);
      this.marcarTodosComoTocados(this.datosEnvioForm);
      this.marcarTodosComoTocados(this.datosPagoForm);
      this.errorMessage = 'Por favor, complete todos los campos requeridos correctamente.';
    }
    
    return formularioValido;
  }

  marcarTodosComoTocados(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      if (control instanceof FormGroup) {
        this.marcarTodosComoTocados(control);
      }
    });
  }

  confirmarPedido(): void {
    if (!this.validarPasoActual()) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const usuarioId = this.usuario?.id;
    if (!usuarioId) {
      this.errorMessage = 'No hay usuario autenticado';
      this.isLoading = false;
      return;
    }

    const tipoEntrega = this.datosEnvioForm.get('tipoEntrega')?.value;
    const costoEnvio = tipoEntrega === 'despacho_domicilio' ? 5000 : 0;

    const pedido = this.crearObjetoPedido(usuarioId, tipoEntrega, costoEnvio);

    this.pedidoService.crearPedido(pedido)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (pedidoId) => {
          this.pedidoId = pedidoId;
          this.procesarPago(pedidoId, pedido.metodoPago);
        },
        error: (err) => {
          this.errorMessage = 'Error al crear el pedido. Por favor, intente nuevamente.';
          this.isLoading = false;
          console.error('Error al crear el pedido', err);
        }
      });
  }

  private crearObjetoPedido(usuarioId: number, tipoEntrega: TipoEntrega, costoEnvio: number) {
    return {
      usuarioId,
      tipoEntrega,
      sucursalId: tipoEntrega === 'retiro_tienda' ? this.datosEnvioForm.get('sucursalId')?.value : null,
      direccion: tipoEntrega === 'despacho_domicilio' ? {
        calle: this.datosEnvioForm.get('calle')?.value,
        numero: this.datosEnvioForm.get('numero')?.value,
        departamento: this.datosEnvioForm.get('departamento')?.value,
        comuna: this.datosEnvioForm.get('comuna')?.value,
        region: this.datosEnvioForm.get('region')?.value,
        codigoPostal: this.datosEnvioForm.get('codigoPostal')?.value
      } : null,
      notas: this.datosEnvioForm.get('notas')?.value ?? '',
      metodoPago: this.datosPagoForm.get('metodoPago')?.value,
      subtotal: this.carrito.subtotal,
      impuestos: this.carrito.impuestos,
      descuentos: this.carrito.descuentos,
      costoEnvio,
      total: this.carrito.total + costoEnvio,
      detalles: this.carrito.items.map(item => ({
        productoId: item.productoId,
        cantidad: item.cantidad,
        precioUnitario: item.precioUnitario,
        subtotal: item.cantidad * item.precioUnitario
      }))
    };
  }

  private procesarPago(pedidoId: number, metodoPago: string): void {
    if (metodoPago === 'mercadopago') {
      this.procesarPagoMercadoPago(pedidoId);
    } else {
      this.procesarPagoTransferencia(pedidoId);
    }
  }

  private procesarPagoMercadoPago(pedidoId: number): void {
    this.pagoService.crearPreferenciaMercadoPago(pedidoId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (respuesta) => {
          window.location.href = respuesta.init_point;
        },
        error: (err) => {
          this.errorMessage = 'Error al procesar el pago con Mercado Pago';
          this.isLoading = false;
          console.error('Error al crear preferencia de Mercado Pago', err);
        }
      });
  }

  private procesarPagoTransferencia(pedidoId: number): void {
    const pago = {
      pedidoId,
      metodo: 'transferencia' as MetodoPago,
      monto: this.carrito.total,
      referenciaTransaccion: ''
    };
    
    this.pagoService.crearPago(pago)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.limpiarCarrito();
          this.pedidoCreado = true;
          this.isLoading = false;
        },
        error: (err) => {
          this.errorMessage = 'Error al procesar el pago por transferencia';
          this.isLoading = false;
          console.error('Error al procesar pago por transferencia', err);
        }
      });
  }

  getNombreSucursal(sucursalId: number): string {
    const sucursal = this.sucursales.find(s => s.id === sucursalId);
    return sucursal ? sucursal.nombre : '';
  }

  getImagenProducto(item: any): string {
    return item.producto?.imagenUrl || 'assets/img/no-image.svg';
  }

  getNombreProducto(item: any): string {
    return item.producto?.nombre || 'Producto sin nombre';
  }

  getSubtotalItem(item: any): number {
    return item.cantidad * item.precioUnitario;
  }

  getTotalConEnvio(): number {
    const costoEnvio = this.datosEnvioForm.get('tipoEntrega')?.value === 'despacho_domicilio' ? 5000 : 0;
    return this.carrito.total + costoEnvio;
  }

  getCostoEnvio(): number {
    return this.datosEnvioForm.get('tipoEntrega')?.value === 'despacho_domicilio' ? 5000 : 0;
  }

  getMetodoPagoNombre(metodo: string): string {
    return metodo === 'mercadopago' ? 'Mercado Pago' : 'Transferencia Bancaria';
  }

  limpiarCarrito(): void {
    this.carritoService.vaciarCarrito().subscribe({
      next: () => {
        this.notificacionService.exito('Carrito vaciado correctamente');
      },
      error: (error: Error) => {
        this.notificacionService.error('Error al vaciar el carrito');
        console.error('Error:', error);
      }
    });
  }
} 