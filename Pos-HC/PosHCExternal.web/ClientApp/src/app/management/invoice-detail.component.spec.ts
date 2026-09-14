import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { PosHsPayment } from '../payment/pos-hs-payment/pos-hc-payment';
import { InvoiceDetailComponent } from './invoice-detail.component';
import { BillingComponent } from './billing.component';
import { AuthService } from './auth.service';
import { DIALOG_DATA } from '../services/dialog-ref';
import { DialogService } from '../services/pos-hs-dialog.service';

describe('Invoice detail modal', () => {
  const invoice = {
    Summary: {
      Invoice: {
        Id: 'invoice-1',
        Number: 'INV-1',
        Currency: 'USD',
        Total: 100,
        Items: [],
        Status: 'Issued',
      },
      Paid: 0,
      Balance: 100,
      Credits: 0,
    },
    Payments: [],
    CreditNotes: [],
  };
  let open: jasmine.Spy;
  let changed: jasmine.Spy;
  beforeEach(() => {
    open = jasmine
      .createSpy('openComponent')
      .and.returnValue({ afterClosed: () => of({ success: true }) });
    changed = jasmine.createSpy('onChanged');
    TestBed.configureTestingModule({
      imports: [InvoiceDetailComponent, BillingComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: DIALOG_DATA,
          useValue: { invoiceId: 'invoice-1', onChanged: changed },
        },
        { provide: DialogService, useValue: { openComponent: open } },
        { provide: AuthService, useValue: { can: () => true } },
      ],
    });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());
  it('opens a separate detail component from the invoice list', () => {
    const list = TestBed.createComponent(BillingComponent).componentInstance;
    list.open('invoice-1');
    expect(open).toHaveBeenCalledWith(
      InvoiceDetailComponent,
      'Invoice details',
      jasmine.objectContaining({ invoiceId: 'invoice-1' }),
    );
  });
  it('loads invoice details and refreshes both detail and list after a payment', () => {
    const fixture = TestBed.createComponent(InvoiceDetailComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/invoice/invoice-1').flush(invoice);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('INV-1');
    fixture.componentInstance.collect();
    expect(open.calls.mostRecent().args[0]).toBe(PosHsPayment);
    expect(open.calls.mostRecent().args[2]).toEqual({ invoiceId: 'invoice-1' });
    expect(open.calls.mostRecent().args[3]).toEqual({ nested: true });
    http.expectOne('/api/invoice/invoice-1').flush(invoice);
    expect(changed).toHaveBeenCalledTimes(1);
  });
  it('keeps a failed load in the modal and offers retry', () => {
    const fixture = TestBed.createComponent(InvoiceDetailComponent);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/invoice/invoice-1')
      .flush(
        { detail: 'Invoice unavailable' },
        { status: 500, statusText: 'Error' },
      );
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('[role="alert"]').textContent,
    ).toContain('Invoice unavailable');
    expect(fixture.nativeElement.textContent).toContain('Retry');
  });

  for (const [kind, endpoint] of [
    ['credits', 'api/invoice/invoice-1/credits'],
    ['refunds', 'api/payment/invoice/invoice-1/refunds'],
  ]) {
    it(`sends ${kind} to the controller that owns the operation`, () => {
      open.and.returnValue({ afterClosed: () => of(undefined) });
      const fixture = TestBed.createComponent(InvoiceDetailComponent);
      fixture.detectChanges();
      TestBed.inject(HttpTestingController)
        .expectOne('/api/invoice/invoice-1')
        .flush(invoice);

      fixture.componentInstance.adjust(kind);

      expect(open.calls.mostRecent().args[2].endpoint).toBe(endpoint);
    });
  }
});
