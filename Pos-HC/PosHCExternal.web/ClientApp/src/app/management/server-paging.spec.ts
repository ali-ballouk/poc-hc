import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter,
} from '@angular/router';
import { of } from 'rxjs';
import { BillingComponent } from './billing.component';
import { ManagementComponent } from './management.component';
import { AuthService } from './auth.service';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';

describe('Backend grid paging', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BillingComponent, ManagementComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: DialogService, useValue: {} },
        { provide: AuthService, useValue: { can: () => true } },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ module: 'patients' })),
            snapshot: {
              queryParamMap: convertToParamMap({ patientId: 'patient-1' }),
            },
          },
        },
      ],
    });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('sends header sort to the API and preserves it when paging', () => {
    const fixture = TestBed.createComponent(BillingComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne(
        '/api/billing/invoices?page=1&pageSize=20&search=&patientId=patient-1',
      )
      .flush({ Items: [{ Id: 'first' }], Total: 105, Page: 1, PageSize: 20 });
    fixture.detectChanges();
    const grid = fixture.debugElement.query(By.directive(GenericGridComponent))
      .componentInstance as GenericGridComponent;
    grid.toggleSort(grid.columns[0]);
    http
      .expectOne(
        '/api/billing/invoices?page=1&pageSize=20&search=&sortBy=Number&sortDirection=asc&patientId=patient-1',
      )
      .flush({ Items: [{ Id: 'sorted' }], Total: 105, Page: 1, PageSize: 20 });
    fixture.detectChanges();
    grid.nextPage();
    http
      .expectOne(
        '/api/billing/invoices?page=2&pageSize=20&search=&sortBy=Number&sortDirection=asc&patientId=patient-1',
      )
      .flush({ Items: [], Total: 105, Page: 2, PageSize: 20 });
    http.verify();
    expect(fixture.componentInstance.sort).toEqual({ columnKey: 'Number', direction: 'asc' });
    expect(fixture.componentInstance.page).toBe(2);
  });

  it('requests invoice pages and page sizes while preserving the patient filter', () => {
    const fixture = TestBed.createComponent(BillingComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne(
        '/api/billing/invoices?page=1&pageSize=20&search=&patientId=patient-1',
      )
      .flush({ Items: [{ Id: 'first' }], Total: 105, Page: 1, PageSize: 20 });
    fixture.detectChanges();
    const grid = fixture.debugElement.query(By.directive(GenericGridComponent))
      .componentInstance as GenericGridComponent;
    grid.nextPage();
    http
      .expectOne(
        '/api/billing/invoices?page=2&pageSize=20&search=&patientId=patient-1',
      )
      .flush({ Items: [{ Id: 'second' }], Total: 105, Page: 2, PageSize: 20 });
    fixture.detectChanges();
    expect(grid.pageIndex).toBe(1);
    expect(grid.pagedRows).toEqual([{ Id: 'second' }]);
    grid.onPageSizeChanged(50);
    http
      .expectOne(
        '/api/billing/invoices?page=1&pageSize=50&search=&patientId=patient-1',
      )
      .flush({ Items: [], Total: 0, Page: 1, PageSize: 50 });
    fixture.detectChanges();
    expect(grid.pageCount).toBe(1);
  });

  it('sends management paging to the API and resets page on backend search', () => {
    const fixture = TestBed.createComponent(ManagementComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne('/api/clinic/patients?page=1&pageSize=20&search=')
      .flush({ Items: [], Total: 75, Page: 1, PageSize: 20 });
    fixture.detectChanges();
    const grid = fixture.debugElement.query(By.directive(GenericGridComponent))
      .componentInstance as GenericGridComponent;
    grid.nextPage();
    http
      .expectOne('/api/clinic/patients?page=2&pageSize=20&search=')
      .flush({ Items: [{ Id: 'page2' }], Total: 75, Page: 2, PageSize: 20 });
    fixture.detectChanges();
    fixture.componentInstance.search = 'Alice & Bob';
    fixture.nativeElement
      .querySelector('form')
      .dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    http
      .expectOne(
        '/api/clinic/patients?page=1&pageSize=20&search=Alice%20%26%20Bob',
      )
      .flush({ Items: [], Total: 0, Page: 1, PageSize: 20 });
    fixture.detectChanges();
    expect(grid.pageIndex).toBe(0);
    expect(fixture.nativeElement.textContent).not.toContain('Previous 50');
  });

  it('cancels stale invoice requests and clears stale rows after an error', () => {
    const fixture = TestBed.createComponent(BillingComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    const oldRequest = http.expectOne(
      '/api/billing/invoices?page=1&pageSize=20&search=&patientId=patient-1',
    );
    fixture.componentInstance.onPageChange({ pageIndex: 1, pageSize: 20 });
    expect(oldRequest.cancelled).toBeTrue();
    http
      .expectOne(
        '/api/billing/invoices?page=2&pageSize=20&search=&patientId=patient-1',
      )
      .flush(
        { detail: 'Unable to load invoices.' },
        { status: 500, statusText: 'Error' },
      );
    expect(fixture.componentInstance.loading).toBeFalse();
    expect(fixture.componentInstance.rows).toEqual([]);
    expect(fixture.componentInstance.error).toBe('Unable to load invoices.');
  });
});
