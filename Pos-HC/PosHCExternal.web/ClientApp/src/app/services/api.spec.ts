import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';

import { BaseAPI } from './base.api';
import { LanguageService } from '../i18n/language';

describe('BaseAPI', () => {
  let service: BaseAPI;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withXhr()), provideHttpClientTesting()],
    });
    service = TestBed.inject(BaseAPI);
    TestBed.inject(LanguageService).setLanguage('en');
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('downloads PDF bytes from the backend URL', () => {
    const http = TestBed.inject(HttpTestingController);
    const pdf = new Blob(['%PDF-1.7'], { type: 'application/pdf' });
    service.downloadPdf('api/invoice/test/print').subscribe((response) => {
      expect(response.body).toBe(pdf);
    });
    const request = http.expectOne('/api/invoice/test/print?language=en');
    expect(request.request.responseType).toBe('blob');
    request.flush(pdf);
    http.verify();
  });
  it('passes the current Arabic selection to both invoice and receipt downloads', () => {
    const http = TestBed.inject(HttpTestingController);
    TestBed.inject(LanguageService).setLanguage('ar');
    for (const endpoint of [
      'api/invoice/test/print',
      'api/billing/payments/test/receipt',
    ]) {
      service.downloadPdf(endpoint).subscribe();
      const request = http.expectOne('/' + endpoint + '?language=ar');
      expect(request.request.params.get('language')).toBe('ar');
      request.flush(new Blob(['%PDF']));
    }
    TestBed.inject(LanguageService).setLanguage('en');
    http.verify();
  });
});
