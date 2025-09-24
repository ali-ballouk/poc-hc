import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsOnaccountpayment } from './pos-hs-onaccountpayment';

describe('PosHsOnaccountpayment', () => {
  let component: PosHsOnaccountpayment;
  let fixture: ComponentFixture<PosHsOnaccountpayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsOnaccountpayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHsOnaccountpayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
