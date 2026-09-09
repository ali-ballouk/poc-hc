import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { DIALOG_DATA, DialogRef } from '../../services/dialog-ref';
import { PosHsPayment } from './pos-hc-payment';

describe('PosHsPayment', () => {
  let component: PosHsPayment;
  let fixture: ComponentFixture<PosHsPayment>;
  let http: HttpTestingController;
  let close: jasmine.Spy;

  beforeEach(async () => {
    close = jasmine.createSpy('close');
    await TestBed.configureTestingModule({
      imports: [PosHsPayment],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DIALOG_DATA, useValue: { invoiceId: 'test-invoice' } },
        { provide: DialogRef, useValue: { close } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(PosHsPayment);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/paymenttype/lookup').flush([
      { Id: 1, Name: 'Cash' },
      { Id: 2, Name: 'Card' },
      { Id: 3, Name: 'Transfer' },
      { Id: 4, Name: 'On account' },
    ]);
    fixture.detectChanges();
  });
  afterEach(() => http.verify());

  function choose(type: number) {
    const selector = fixture.nativeElement.querySelector(
      'app-generic-selector',
    );
    if (type === 0) {
      selector.querySelector('button[title="Clear selection"]').click();
    } else {
      selector.querySelector('.dropdown-toggle').click();
      fixture.detectChanges();
      selector.querySelectorAll('[role="option"]')[type - 1].click();
    }
    fixture.detectChanges();
  }

  function enter(id: string, value: string) {
    const input = fixture.nativeElement.querySelector(
      '#' + id,
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('renders all payment forms through the generic selector', () => {
    for (const [type, id] of [
      [1, 'cash-drawer'],
      [2, 'card-expiry'],
      [3, 'transfer-bank'],
      [4, 'account-id'],
    ] as const) {
      choose(type);
      expect(fixture.nativeElement.querySelector('#' + id)).not.toBeNull();
    }
    choose(0);
    expect(component.wrapper.getData()).toBeNull();
  });

  it('waits for a successful payment response before closing', () => {
    choose(1);
    enter('cash-drawer', 'drawer-1');
    component.save();
    const request = http.expectOne('/api/payment');
    expect(request.request.body).toEqual({
      RequestId: component.requestId,
      InvoiceId: 'test-invoice',
      PaymentTypeId: 1,
      Settings: { paymentType: 'cash', CashDrawerId: 'drawer-1' },
    });
    expect(close).not.toHaveBeenCalled();
    component.save();
    http.expectNone('/api/payment');
    request.flush({});
    expect(close).toHaveBeenCalledOnceWith({ success: true });
  });

  it('keeps entered payment data when saving fails', () => {
    choose(2);
    enter('card-last-four', '1234');
    enter('card-token', 'test-token');
    enter('card-expiry', '2028-12');
    component.save();
    const request = http.expectOne('/api/payment');
    expect(request.request.body.Settings).toEqual({
      paymentType: 'card',
      CardNumber: '1234',
      Expiry: '2028-12',
      Token: 'test-token',
    });
    request.flush({}, { status: 500, statusText: 'Server error' });
    fixture.detectChanges();
    expect(close).not.toHaveBeenCalled();
    expect(
      fixture.nativeElement.querySelector('[role="alert"]').textContent,
    ).toContain('could not be saved');
    expect(
      (
        fixture.nativeElement.querySelector(
          '#card-last-four',
        ) as HTMLInputElement
      ).value,
    ).toBe('1234');
    expect(component.saving).toBeFalse();
  });
});
