import { TranslatePipe } from '../i18n/language';
import {
  Component,
  OnInit,
  signal,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BaseAPI } from '../services/base.api';

import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import { GenericGridColumnComponent } from '../ui-shared/components/generic-grid/generic-grid-column.component';
import { GridAction } from '../ui-shared/components/generic-grid/generic-grid.models';

interface Invoice {
  InvoiceId: string;
  InvoiceDate: Date;
  DoctorId: string;
  DoctorName: string;
  PatientId: string;
  PatientName: string;
  Discount: Number;
  DoctorFee: Number;
  Total: Number;
  Items: any[];
}
@Component({
  selector: 'pos-hs-invoices',
  imports: [
    TranslatePipe,
    CommonModule,
    GenericGridComponent,
    GenericGridColumnComponent,
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-invoices.html',
})
export class PosHcInvoices implements OnInit {
  constructor(private api: BaseAPI) {}

  rows: Invoice[] = [];
  loading = true;
  downloadError = '';
  readonly rowId = (row: Invoice) => row.InvoiceId;
  readonly actions: GridAction<Invoice>[] = [
    {
      label: 'Download PDF',
      color: 'primary',
      handler: (row) => this.downloadInvoice(row.InvoiceId),
    },
  ];

  ngOnInit(): void {
    this.api.get<any[]>('api/invoice/lookup').subscribe({
      next: (res) => {
        this.bindaData(res);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading invoices', err);
      },
    });
  }

  bindaData(response: Invoice[]) {
    this.rows = response.map((x) => ({
      ...x,
      InvoiceDate: new Date(x.InvoiceDate), // string → Date
    }));
  }

  downloadInvoice(id: string) {
    this.downloadError = '';
    const thisApiUrl = `api/invoice/${id}/print`;
    this.api.downloadPdf(thisApiUrl).subscribe({
      next: (res) => {
        const blob = res.body;
        if (
          !blob?.size ||
          blob.type.split(';')[0].toLowerCase() !== 'application/pdf'
        ) {
          this.downloadError =
            'The server did not return a PDF. Please try again.';
          return;
        }
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice-${id}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: () => {
        this.downloadError =
          'Unable to download the invoice. Please try again.';
      },
    });
  }
}
