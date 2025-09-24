import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsCashpayment } from './pos-hs-cashpayment';

describe('PosHsCashpayment', () => {
  let component: PosHsCashpayment;
  let fixture: ComponentFixture<PosHsCashpayment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsCashpayment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHsCashpayment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
