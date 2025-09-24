
import { Component, Inject, ViewChild } from '@angular/core';

import { Type } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
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
}
interface Payment {
  InvoiceId: string;
  PaymentTypeId: Number,
  Settings: any
}

@Component({
  selector: 'app-pos-hc-payment',
  standalone: true,
  imports: [PaymentTypeSelectorComponent, PosHsWrapperComponent],
  templateUrl: './pos-hc-payment.html',

})

export class PosHsPayment {
  selectedPaymentTypeId: Number | null = 0;
  selectedComponent: Type<any> | null = null;

  @ViewChild(PosHsWrapperComponent) wrapper!: PosHsWrapperComponent;

  constructor(
    private dialogRef: MatDialogRef<PosHsPayment>,
    @Inject(MAT_DIALOG_DATA) public data: any, private api: BaseAPI
  ) { }

  save() {
    const paymentData = this.wrapper?.getData?.();
    console.log('Payment Data', this.data.invoiceId); 
    const payload = {
      InvoiceId: this.data.invoiceId,
      PaymentTypeId: this.selectedPaymentTypeId,
      Settings: paymentData
    }
    this.api.post<Payment>('api/payment', payload).subscribe({
      next: (res) => {
        console.log('Submitted successfully', res);
      },
      error: (err) => console.error('Error loading patients', err)
    });

    this.dialogRef.close({
      success: true
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
