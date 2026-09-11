import { TranslatePipe } from '../i18n/language';
import {
  Component,
  inject,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BaseAPI } from '../services/base.api';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridColumn,
  GridAction,
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
export class BillingComponent implements OnInit {
  api = inject(BaseAPI);
  dialog = inject(DialogService);
  route = inject(ActivatedRoute);
  rows: any[] = [];
  page = 1;
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
    this.loading = true;
    this.api
      .get<any>(
        'api/billing/invoices?page=' +
          this.page +
          '&search=' +
          encodeURIComponent(this.search) +
          (this.patientId ? '&patientId=' + this.patientId : ''),
      )
      .subscribe({
        next: (r) => {
          this.rows = r.Items;
          this.total = r.Total;
          this.loading = false;
        },
        error: (e) => {
          this.loading = false;
          this.error = e.error?.detail || 'Unable to load invoices.';
        },
      });
  }
  open(id: string) {
    this.dialog.openComponent(InvoiceDetailComponent, 'Invoice details', {
      invoiceId: id,
      onChanged: () => this.load(),
    });
  }
}
