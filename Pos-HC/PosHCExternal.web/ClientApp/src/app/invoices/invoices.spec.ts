import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { PosHcInvoices } from './pos-hc-invoices';

describe('PosHcInvoices', () => {
  it('rejects HTML responses instead of saving a corrupt PDF', async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcInvoices],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(PosHcInvoices);
    const http = TestBed.inject(HttpTestingController);
    const createUrl = spyOn(URL, 'createObjectURL');
    fixture.componentInstance.downloadInvoice('invoice-1');
    http
      .expectOne('/api/invoice/invoice-1/print?language=en')
      .flush(new Blob(['<!doctype html>'], { type: 'text/html' }));
    expect(createUrl).not.toHaveBeenCalled();
    expect(fixture.componentInstance.downloadError).toContain(
      'did not return a PDF',
    );
    http.verify();
  });

  it('renders projected grid columns with date/time, currencies and the download action', async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcInvoices],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(PosHcInvoices);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/invoice/lookup').flush([
      {
        InvoiceId: 'invoice-1',
        InvoiceDate: '2026-09-06T10:30:00',
        DoctorId: 'd',
        DoctorName: 'Doctor',
        PatientId: 'p',
        PatientName: 'Patient',
        DoctorFee: 35,
        Discount: 5,
        Total: 30,
        Items: [],
      },
    ]);
    fixture.detectChanges();
    const row = fixture.nativeElement.querySelector('.data-row');
    expect(row.textContent).toContain('2026-09-06 10:30');
    expect(row.textContent).toContain('35.00');
    expect(row.textContent).toContain('30.00');
    const download = spyOn(fixture.componentInstance, 'downloadInvoice');
    row.querySelector('button').click();
    expect(download).toHaveBeenCalledOnceWith('invoice-1');
    http.verify();
  });
});
