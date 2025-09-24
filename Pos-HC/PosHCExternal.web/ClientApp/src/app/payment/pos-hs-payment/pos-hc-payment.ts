
import { Component, Inject } from '@angular/core';

import { Type } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PosHsWrapperComponent } from '../../../app/ui-shared/pos-hs-wrapper-component/pos-hs-wrapper-component';

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

@Component({
  selector: 'app-pos-hc-payment',
  standalone: true,
  imports: [PaymentTypeSelectorComponent, PosHsWrapperComponent],
  templateUrl: './pos-hc-payment.html',

})
export class PosHsPayment {

  selectedPaymentTypeId: Number | null = 0;
  selectedComponent: Type<any> | null = null;
  constructor(
    private dialogRef: MatDialogRef<PosHsPayment>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  save() {
    this.dialogRef.close({ success: true, id: this.data?.invoiceId });
  }

  onPaymentTypeSelected(value: any) {
    this.selectedPaymentTypeId = value;
    this.selectedComponent = PaymentComponentMap[value as number] || null;
  }

  close() {
    this.dialogRef.close();
  }
}
