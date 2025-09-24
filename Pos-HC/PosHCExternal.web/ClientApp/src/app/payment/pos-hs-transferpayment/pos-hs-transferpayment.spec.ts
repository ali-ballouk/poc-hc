import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsTransferpayment } from './pos-hs-transferpayment';

describe('PosHsTransferpayment', () => {
  let component: PosHsTransferpayment;
  let fixture: ComponentFixture<PosHsTransferpayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsTransferpayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHsTransferpayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
