import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { DIALOG_DATA, DialogRef } from '../../services/dialog-ref';
import { PosHsPayment } from './pos-hc-payment';

describe('Shared cash payment', () => {
  let http: HttpTestingController;
  let close: jasmine.Spy;
  beforeEach(() => {
    close = jasmine.createSpy('close');
    TestBed.configureTestingModule({
      imports: [PosHsPayment],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DIALOG_DATA, useValue: { invoiceId: 'invoice-1' } },
        { provide: DialogRef, useValue: { close } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  function setup(currency = 'USD', status = 'Issued', balance = 65) {
    const fixture = TestBed.createComponent(PosHsPayment);
    fixture.detectChanges();
    expect(fixture.componentInstance.canSave).toBeFalse();
    http.expectOne('/api/invoice/invoice-1').flush({
      Summary: {
        Invoice: {
          Id: 'invoice-1',
          Number: 1001,
          PatientName: 'Demo',
          Currency: currency,
          Status: status,
        },
        Balance: balance,
      },
    });
    fixture.detectChanges();
    return fixture;
  }
  it('loads the current balance and shows cash without a method selector', () => {
    const fixture = setup();
    expect(fixture.componentInstance.amount).toBe(65);
    expect(fixture.nativeElement.textContent).toContain('Cash');
    expect(
      fixture.nativeElement.querySelector('app-generic-selector'),
    ).toBeNull();
    expect(
      fixture.nativeElement.querySelector('#cash-payment-amount'),
    ).not.toBeNull();
  });
  it('records a partial cash payment once and closes only after success', () => {
    const component = setup().componentInstance;
    component.amount = 30;
    component.save();
    const request = http.expectOne('/api/payment');
    expect(request.request.body).toEqual({
      RequestId: component.requestId,
      InvoiceId: 'invoice-1',
      Amount: 30,
      PaymentTypeId: 1,
      Settings: { paymentType: 'cash', CashDrawerId: 'current' },
    });
    component.save();
    http.expectNone('/api/payment');
    expect(close).not.toHaveBeenCalled();
    request.flush({});
    expect(close).toHaveBeenCalledOnceWith({ success: true });
  });
  it('preserves the amount and retry key after a failure', () => {
    const fixture = setup();
    const component = fixture.componentInstance;
    component.amount = 20;
    component.save();
    const first = http.expectOne('/api/payment');
    first.flush(
      { detail: 'Open a cash shift for this currency first.' },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();
    expect(close).not.toHaveBeenCalled();
    expect(component.amount).toBe(20);
    expect(
      fixture.nativeElement.querySelector('[role="alert"]'),
    ).not.toBeNull();
    component.save();
    const retry = http.expectOne('/api/payment');
    expect(retry.request.body.RequestId).toBe(first.request.body.RequestId);
    retry.flush({});
  });
  it('rejects zero, excess, nonfinite and invalid currency precision amounts', () => {
    const component = setup('LBP').componentInstance;
    for (const amount of [0, -1, 66, 1.5, NaN, Infinity]) {
      component.amount = amount;
      expect(component.canSave).toBeFalse();
      component.save();
    }
    http.expectNone('/api/payment');
    component.amount = 10;
    expect(component.canSave).toBeTrue();
  });
  it('does not collect against a draft or a paid invoice', () => {
    const component = setup('USD', 'Draft').componentInstance;
    expect(component.canSave).toBeFalse();
    component.summary!.Invoice.Status = 'Issued';
    component.summary!.Balance = 0;
    expect(component.canSave).toBeFalse();
  });
});
