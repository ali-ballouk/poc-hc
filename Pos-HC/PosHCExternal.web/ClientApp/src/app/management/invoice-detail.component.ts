import { TranslatePipe, LocalNumberPipe } from '../i18n/language';
import {
  Component,
  inject,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DIALOG_DATA } from '../services/dialog-ref';
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BaseAPI } from '../services/base.api';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridColumn,
  GridAction,
} from '../ui-shared/components/generic-grid/generic-grid.models';
import { RecordFormComponent } from './record-form.component';
import { Field, amount } from './module-definitions';
import { PosHsPayment } from '../payment/pos-hs-payment/pos-hc-payment';
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
  templateUrl: './invoice-detail.component.html',
})
export class InvoiceDetailComponent implements OnInit {
  api = inject(BaseAPI);
  dialog = inject(DialogService);
  auth = inject(AuthService);
  data = inject(DIALOG_DATA) as { invoiceId: string; onChanged?: () => void };
  private destroyRef = inject(DestroyRef);
  detail: any;
  error = '';
  loading = false;
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
    this.load();
  }
  load() {
    this.loading = true;
    this.error = '';
    this.api
      .get('api/billing/invoices/' + this.data.invoiceId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.detail = result;
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.error = error.error?.detail || 'Unable to load invoice.';
        },
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
      .openComponent(
        RecordFormComponent,
        title,
        {
          fields,
          endpoint,
          value,
          prepare,
        },
        { nested: true },
      )
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((r) => {
        if (r) {
          this.load();
          this.data.onChanged?.();
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
    this.dialog
      .openComponent(
        PosHsPayment,
        'Record payment taken from patient',
        { invoiceId: this.data.invoiceId },
        { nested: true },
      )
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result) => {
        if (result?.success) {
          this.load();
          this.data.onChanged?.();
        }
      });
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
