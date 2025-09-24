import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsPayment } from './pos-hc-payment';

describe('PosHsPayment', () => {
  let component: PosHsPayment;
  let fixture: ComponentFixture<PosHsPayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsPayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHsPayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
