
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PaymentTypeSelectorComponent } from '../../payment-type/pos-hc-paymenttype/pos-hc-paymenttype';

@Component({
  selector: 'app-pos-hc-payment',
  standalone: true,
  imports: [PaymentTypeSelectorComponent],
  templateUrl: './pos-hc-payment.html',

})
export class PosHsPayment {

  selectedPaymentTypeId: Number | null = 0;

  constructor(
    private dialogRef: MatDialogRef<PosHsPayment>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  save() {
    this.dialogRef.close({ success: true, id: this.data?.invoiceId });
  }

  onPaymentTypeSelected(value: any) {
    this.selectedPaymentTypeId = value;
  }

  close() {
    this.dialogRef.close();
  }
}
