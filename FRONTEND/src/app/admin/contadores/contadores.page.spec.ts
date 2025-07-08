import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ContadoresPage } from './contadores.page';

describe('ContadoresPage', () => {
  let component: ContadoresPage;
  let fixture: ComponentFixture<ContadoresPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(ContadoresPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
