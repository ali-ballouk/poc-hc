
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-pos-hs-payment',
  standalone: true,
  templateUrl: './pos-hs-payment.html',

})
export class PosHsPayment {
  constructor(
    private dialogRef: MatDialogRef<PosHsPayment>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  save() {
    this.dialogRef.close({ success: true, id: this.data?.invoiceId });
  }

  close() {
    this.dialogRef.close();
  }
}
