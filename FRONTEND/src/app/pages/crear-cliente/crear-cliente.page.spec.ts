import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CrearClientePage } from './crear-cliente.page';

describe('CrearClientePage', () => {
  let component: CrearClientePage;
  let fixture: ComponentFixture<CrearClientePage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(CrearClientePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
