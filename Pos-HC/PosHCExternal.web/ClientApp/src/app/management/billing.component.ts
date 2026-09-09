import { TranslatePipe, LocalNumberPipe } from '../i18n/language';
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
import { RecordFormComponent } from './record-form.component';
import { Field, amount } from './module-definitions';
import { AuthService } from './auth.service';
const cols = (...keys: string[]): GridColumn<any>[] =>
  keys.map((key) => ({ key, header: key.replace(/([a-z])([A-Z])/g, '$1 $2') }));
@Component({
  standalone: true,
  imports: [
    TranslatePipe,
    LocalNumberPipe,
    CommonModule,
    FormsModule,
    GenericGridComponent,
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './billing.component.html',
})
export class BillingComponent implements OnInit {
  api = inject(BaseAPI);
  dialog = inject(DialogService);
  auth = inject(AuthService);
  route = inject(ActivatedRoute);
  rows: any[] = [];
  detail: any;
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
  itemColumns = cols('Name', 'Quantity', 'UnitPrice', 'LineTotal');
  paymentColumns = cols(
    'Kind',
    'Amount',
    'Currency',
    'PaymentTypeId',
    'Reference',
    'PaymentDate',
  );
  creditColumns = cols('Amount', 'Reason', 'CreatedAt');
  actions: GridAction<any>[] = [
    { label: 'Open', handler: (row) => this.open(row.Id) },
  ];
  paymentActions: GridAction<any>[] = [
    {
      label: 'Receipt PDF',
      handler: (row) =>
        this.download(
          'api/billing/payments/' + row.Id + '/receipt',
          'Receipt-' + row.Id + '.pdf',
        ),
    },
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
    this.api.get('api/billing/invoices/' + id).subscribe({
      next: (r) => (this.detail = r),
      error: (e) => (this.error = e.error?.detail || 'Unable to load invoice.'),
    });
  }
  form(
    title: string,
    fields: Field[],
    endpoint: string,
    value: any,
    prepare?: any,
  ) {
    this.dialog
      .openComponent(RecordFormComponent, title, {
        fields,
        endpoint,
        value,
        prepare,
      })
      .afterClosed()
      .subscribe((r) => {
        if (r) {
          this.open(this.detail.Summary.Invoice.Id);
          this.load();
        }
      });
  }
  status(status: string) {
    this.form(
      status === 'Void' ? 'Void invoice' : 'Issue invoice',
      status === 'Void'
        ? [{ key: 'Reason', label: 'Cancellation reason', required: true }]
        : [],
      'api/billing/invoices/' + this.detail.Summary.Invoice.Id + '/status',
      { Status: status },
    );
  }
  collect() {
    this.form(
      'Record payment taken from patient',
      [
        amount,
        {
          key: 'PaymentTypeId',
          label: 'Method',
          type: 'select',
          required: true,
          options: [
            { Id: 1, Name: 'Cash' },
            { Id: 2, Name: 'External card terminal' },
            { Id: 3, Name: 'Bank transfer' },
          ],
        },
        { key: 'Reference', label: 'Terminal or transfer reference' },
        { key: 'Last4', label: 'Last four card digits (card only)' },
        { key: 'Bank', label: 'Bank name (transfer only)' },
      ],
      'api/billing/payments',
      {
        Amount: this.detail.Summary.Balance,
        PaymentTypeId: 1,
        InvoiceId: this.detail.Summary.Invoice.Id,
        RequestId: crypto.randomUUID(),
      },
      (v: any) => ({
        ...v,
        Settings:
          v.PaymentTypeId === 1
            ? { paymentType: 'cash', CashDrawerId: 'current' }
            : v.PaymentTypeId === 2
              ? {
                  paymentType: 'card',
                  CardNumber: v.Last4,
                  Expiry: 'external',
                  Token: v.Reference,
                }
              : {
                  paymentType: 'transfer',
                  Banke: v.Bank,
                  ReferenceNumber: v.Reference,
                },
      }),
    );
  }
  adjust(kind: string) {
    this.form(
      kind === 'credits' ? 'Create credit note' : 'Record completed refund',
      [
        amount,
        {
          key: 'Reason',
          label: 'Reason / external refund reference',
          required: true,
        },
        ...(kind === 'refunds'
          ? [
              {
                key: 'PaymentTypeId',
                label: 'Refund method',
                type: 'select',
                required: true,
                options: [
                  { Id: 1, Name: 'Cash' },
                  { Id: 2, Name: 'External card terminal' },
                  { Id: 3, Name: 'Bank transfer' },
                ],
              },
            ]
          : []),
      ],
      'api/billing/invoices/' + this.detail.Summary.Invoice.Id + '/' + kind,
      {
        Amount: kind === 'refunds' ? -this.detail.Summary.Balance : 0,
        PaymentTypeId: 1,
        RequestId: crypto.randomUUID(),
      },
    );
  }
  download(endpoint: string, name: string) {
    this.api.downloadPdf(endpoint).subscribe({
      next: (r) => {
        if (!r.body?.size || !r.body.type.startsWith('application/pdf')) {
          this.error = 'The server did not return a PDF.';
          return;
        }
        const url = URL.createObjectURL(r.body);
        const a = document.createElement('a');
        a.href = url;
        a.download = name;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 60000);
      },
      error: (e) => (this.error = e.error?.detail || 'Unable to download PDF.'),
    });
  }
}
