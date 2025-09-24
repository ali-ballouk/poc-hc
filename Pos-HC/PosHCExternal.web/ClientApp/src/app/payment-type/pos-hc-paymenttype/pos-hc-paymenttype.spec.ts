import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHcPaymenttype } from './pos-hc-paymenttype';

describe('PosHcPaymenttype', () => {
  let component: PosHcPaymenttype;
  let fixture: ComponentFixture<PosHcPaymenttype>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcPaymenttype]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHcPaymenttype);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
