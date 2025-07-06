import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CrearPedidoBodegaPage } from './crear-pedido-bodega.page';

describe('CrearPedidoBodegaPage', () => {
  let component: CrearPedidoBodegaPage;
  let fixture: ComponentFixture<CrearPedidoBodegaPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(CrearPedidoBodegaPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
