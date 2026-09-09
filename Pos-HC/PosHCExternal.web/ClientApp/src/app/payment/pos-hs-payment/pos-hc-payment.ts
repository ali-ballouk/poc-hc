import { TranslatePipe } from '../../i18n/language';
import {
  Component,
  Inject,
  ViewChild,
  ChangeDetectionStrategy,
} from '@angular/core';

import { Type } from '@angular/core';
import { DialogRef, DIALOG_DATA } from '../../services/dialog-ref';
import { PosHsWrapperComponent } from '../../../app/ui-shared/pos-hs-wrapper-component/pos-hs-wrapper-component';
import { BaseAPI } from '../../services/base.api';

import { PosHsCashpayment } from '../pos-hs-cashpayment/pos-hs-cashpayment';
import { PosHsCardpayment } from '../pos-hs-cardpayment/pos-hs-cardpayment';
import { PosHsTransferpayment } from '../pos-hs-transferpayment/pos-hs-transferpayment';
import { PosHsOnaccountpayment } from '../pos-hs-onaccountpayment/pos-hs-onaccountpayment';

import { PaymentTypeSelectorComponent } from '../../payment-type/pos-hc-paymenttype/pos-hc-paymenttype';

export const PaymentComponentMap: Record<number, Type<any>> = {
  1: PosHsCashpayment,
  2: PosHsCardpayment,
  3: PosHsTransferpayment,
  4: PosHsOnaccountpayment,
};
interface Payment {
  InvoiceId: string;
  PaymentTypeId: Number;
  Settings: any;
}

@Component({
  selector: 'app-pos-hc-payment',
  standalone: true,
  imports: [TranslatePipe, PaymentTypeSelectorComponent, PosHsWrapperComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-payment.html',
})
export class PosHsPayment {
  selectedPaymentTypeId: number | null = null;
  selectedComponent: Type<any> | null = null;

  @ViewChild(PosHsWrapperComponent) wrapper!: PosHsWrapperComponent;

  constructor(
    private dialogRef: DialogRef,
    @Inject(DIALOG_DATA) public data: any,
    private api: BaseAPI,
  ) {}

  saving = false;
  errorMessage = '';
  requestId = crypto.randomUUID();

  save() {
    if (this.saving || !this.selectedComponent) return;
    this.saving = true;
    this.errorMessage = '';
    const paymentData = this.wrapper?.getData?.();
    console.log('Payment Data', this.data.invoiceId);
    const payload = {
      RequestId: this.requestId,
      InvoiceId: this.data.invoiceId,
      PaymentTypeId: this.selectedPaymentTypeId,
      Settings: paymentData,
    };
    this.api.post<Payment>('api/payment', payload).subscribe({
      next: (res) => {
        this.saving = false;
        this.dialogRef.close({ success: true });
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage =
          err.error?.detail || 'Payment could not be saved. Please try again.';
      },
    });
  }

  onPaymentTypeSelected(value: any) {
    this.selectedPaymentTypeId = value;
    this.selectedComponent = PaymentComponentMap[value as number] || null;
  }

  close() {
    this.dialogRef.close();
  }
}
