import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CheckoutPasosComponent } from './checkout-pasos.component';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { CarritoService } from '../../../servicios/carrito.service';
import { AuthService } from '../../../servicios/auth.service';
import { PedidoService } from '../../../servicios/pedido.service';
import { PagoService } from '../../../servicios/pago.service';
import { of } from 'rxjs';

describe('CheckoutPasosComponent', () => {
  let component: CheckoutPasosComponent;
  let fixture: ComponentFixture<CheckoutPasosComponent>;
  let carritoServiceSpy: jasmine.SpyObj<CarritoService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let pedidoServiceSpy: jasmine.SpyObj<PedidoService>;
  let pagoServiceSpy: jasmine.SpyObj<PagoService>;

  beforeEach(async () => {
    const carritoSpy = jasmine.createSpyObj('CarritoService', ['carrito$']);
    const authSpy = jasmine.createSpyObj('AuthService', ['usuario$']);
    const pedidoSpy = jasmine.createSpyObj('PedidoService', ['crearPedido']);
    const pagoSpy = jasmine.createSpyObj('PagoService', ['crearPreferenciaMercadoPago', 'crearPago']);

    carritoSpy.carrito$ = of({
      items: [],
      subtotal: 0,
      impuestos: 0,
      descuentos: 0,
      total: 0
    });

    authSpy.usuario$ = of(null);

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        RouterTestingModule
      ],
      declarations: [ CheckoutPasosComponent ],
      providers: [
        { provide: CarritoService, useValue: carritoSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: PedidoService, useValue: pedidoSpy },
        { provide: PagoService, useValue: pagoSpy }
      ]
    })
    .compileComponents();

    carritoServiceSpy = TestBed.inject(CarritoService) as jasmine.SpyObj<CarritoService>;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    pedidoServiceSpy = TestBed.inject(PedidoService) as jasmine.SpyObj<PedidoService>;
    pagoServiceSpy = TestBed.inject(PagoService) as jasmine.SpyObj<PagoService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckoutPasosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with paso 1', () => {
    expect(component.pasoActual).toBe(1);
  });

  it('should have valid initial form states', () => {
    expect(component.datosClienteForm).toBeTruthy();
    expect(component.datosEnvioForm).toBeTruthy();
    expect(component.datosPagoForm).toBeTruthy();
  });
}); 