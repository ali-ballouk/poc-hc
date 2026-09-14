import { TranslatePipe } from '../i18n/language';
import {
  Component,
  inject,
  OnInit,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { BaseAPI } from '../services/base.api';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridColumn,
  GridAction,
  GridPageChange,
  GridSort,
  GridPageResult,
} from '../ui-shared/components/generic-grid/generic-grid.models';
import { InvoiceDetailComponent } from './invoice-detail.component';
const cols = (...keys: string[]): GridColumn<any>[] =>
  keys.map((key) => ({ key, header: key.replace(/([a-z])([A-Z])/g, '$1 $2') }));
@Component({
  standalone: true,
  imports: [TranslatePipe, CommonModule, FormsModule, GenericGridComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './billing.component.html',
})
export class BillingComponent implements OnInit, OnDestroy {
  api = inject(BaseAPI);
  dialog = inject(DialogService);
  route = inject(ActivatedRoute);
  rows: any[] = [];
  sort?: GridSort<any>;
  page = 1;
  pageSize = 20;
  readonly pageSizeOptions = [10, 20, 50, 100];
  private request?: Subscription;
  total = 0;
  search = '';
  patientId = '';
  error = '';
  loading = false;
  rowId = (row: any) => row.Id;
  columns: GridColumn<any>[] = [
    ...cols(
      'Number',
      'PatientName',
      'DoctorName',
      'Currency',
      'Total',
      'Paid',
      'Balance',
      'PaymentStatus',
    ),
    { key: 'CreatedAt', header: 'Created', type: 'date' },
  ];
  actions: GridAction<any>[] = [
    { label: 'Open', handler: (row) => this.open(row.Id) },
  ];
  ngOnInit() {
    this.patientId = this.route.snapshot.queryParamMap.get('patientId') || '';
    this.load();
  }
  load() {
    this.request?.unsubscribe();
    this.loading = true;
    this.error = '';
    this.request = this.api
      .get<
        GridPageResult<any>
      >('api/billing/invoices?page=' + this.page + '&pageSize=' + this.pageSize + '&search=' + encodeURIComponent(this.search) + this.sortQuery() + (this.patientId ? '&patientId=' + this.patientId : ''))
      .subscribe({
        next: (r) => {
          this.rows = r.Items;
          this.total = r.Total;
          this.page = r.Page;
          this.pageSize = r.PageSize;
          this.loading = false;
        },
        error: (e) => {
          this.loading = false;
          this.rows = [];
          this.error = e.error?.detail || 'Unable to load invoices.';
        },
      });
  }
  sortQuery() {
    return this.sort
      ? '&sortBy=' +
          encodeURIComponent(String(this.sort.columnKey)) +
          '&sortDirection=' +
          this.sort.direction
      : '';
  }
  onSortChange(sort: GridSort<any>) {
    this.sort = sort;
    this.page = 1;
    this.load();
  }
  onPageChange(event: GridPageChange) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
  }
  ngOnDestroy() {
    this.request?.unsubscribe();
  }
  open(id: string) {
    this.dialog.openComponent(InvoiceDetailComponent, 'Invoice details', {
      invoiceId: id,
      onChanged: () => this.load(),
    });
  }
}
