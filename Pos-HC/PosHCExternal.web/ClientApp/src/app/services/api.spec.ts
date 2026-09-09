import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';

import { BaseAPI } from './base.api';

describe('BaseAPI', () => {
  let service: BaseAPI;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withXhr()), provideHttpClientTesting()],
    });
    service = TestBed.inject(BaseAPI);
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
    const request = http.expectOne('/api/invoice/test/print');
    expect(request.request.responseType).toBe('blob');
    request.flush(pdf);
    http.verify();
  });
});
