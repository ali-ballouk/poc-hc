import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsCardpayment } from './pos-hs-cardpayment';

describe('PosHsCardpayment', () => {
  let component: PosHsCardpayment;
  let fixture: ComponentFixture<PosHsCardpayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsCardpayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHsCardpayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
