import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActualizarProductoPage } from './actualizar-producto.page';

describe('ActualizarProductoPage', () => {
  let component: ActualizarProductoPage;
  let fixture: ComponentFixture<ActualizarProductoPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(ActualizarProductoPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
