import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DetallePagoPage } from './detalle-pago.page';

describe('DetallePagoPage', () => {
  let component: DetallePagoPage;
  let fixture: ComponentFixture<DetallePagoPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(DetallePagoPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
