import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RetornoPagoComponent } from './retorno-pago.component';

describe('RetornoPagoComponent', () => {
  let component: RetornoPagoComponent;
  let fixture: ComponentFixture<RetornoPagoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RetornoPagoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RetornoPagoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
