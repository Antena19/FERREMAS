import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BodeguerosPage } from './bodegueros.page';

describe('BodeguerosPage', () => {
  let component: BodeguerosPage;
  let fixture: ComponentFixture<BodeguerosPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(BodeguerosPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
