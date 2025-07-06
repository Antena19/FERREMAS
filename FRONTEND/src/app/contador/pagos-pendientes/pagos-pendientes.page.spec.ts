import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PagosPendientesPage } from './pagos-pendientes.page';

describe('PagosPendientesPage', () => {
  let component: PagosPendientesPage;
  let fixture: ComponentFixture<PagosPendientesPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(PagosPendientesPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
