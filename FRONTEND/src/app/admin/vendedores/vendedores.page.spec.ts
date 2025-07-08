import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VendedoresPage } from './vendedores.page';

describe('VendedoresPage', () => {
  let component: VendedoresPage;
  let fixture: ComponentFixture<VendedoresPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(VendedoresPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
