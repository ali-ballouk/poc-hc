import {
  Component,
  DestroyRef,
  Inject,
  OnInit,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe, LocalNumberPipe } from '../../i18n/language';
import { DialogRef, DIALOG_DATA } from '../../services/dialog-ref';
import { BaseAPI } from '../../services/base.api';

interface InvoiceSummary {
  Invoice: {
    Id: string;
    Number: number;
    PatientName: string;
    Currency: string;
    Status: string;
  };
  Balance: number;
}
@Component({
  selector: 'app-pos-hc-payment',
  standalone: true,
  imports: [TranslatePipe, LocalNumberPipe, FormsModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-payment.html',
})
export class PosHsPayment implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  summary?: InvoiceSummary;
  amount: number | null = null;
  loading = false;
  saving = false;
  errorMessage = '';
  readonly requestId = crypto.randomUUID();
  constructor(
    private dialogRef: DialogRef,
    @Inject(DIALOG_DATA) public data: { invoiceId: string },
    private api: BaseAPI,
  ) {}
  ngOnInit() {
    this.load();
  }
  load() {
    this.loading = true;
    this.errorMessage = '';
    this.api
      .get<{ Summary: InvoiceSummary }>(
        'api/billing/invoices/' + this.data.invoiceId,
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.summary = result.Summary;
          this.amount = Math.max(0, result.Summary.Balance);
          this.loading = false;
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.detail || 'Unable to load invoice.';
        },
      });
  }
  get step() {
    return this.summary?.Invoice.Currency === 'LBP' ? 1 : 0.01;
  }
  get canSave() {
    const value = this.amount;
    const scale = this.summary?.Invoice.Currency === 'LBP' ? 1 : 100;
    return (
      !this.loading &&
      !this.saving &&
      this.summary?.Invoice.Status === 'Issued' &&
      typeof value === 'number' &&
      Number.isFinite(value) &&
      value > 0 &&
      value <= this.summary.Balance &&
      Math.abs(value * scale - Math.round(value * scale)) < 0.000001
    );
  }
  save() {
    if (!this.canSave) return;
    this.saving = true;
    this.errorMessage = '';
    this.api
      .post('api/billing/payments', {
        RequestId: this.requestId,
        InvoiceId: this.data.invoiceId,
        Amount: this.amount,
        PaymentTypeId: 1,
        Settings: { paymentType: 'cash', CashDrawerId: 'current' },
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving = false;
          this.dialogRef.close({ success: true });
        },
        error: (err) => {
          this.saving = false;
          this.errorMessage =
            err.error?.detail ||
            'Payment could not be saved. Please try again.';
        },
      });
  }
  close() {
    if (!this.saving) this.dialogRef.close();
  }
}
