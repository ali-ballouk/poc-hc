import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-transferpayment',
  standalone: true,
  imports: [FormsModule], 
  templateUrl: './pos-hs-transferpayment.html',
  styleUrl: './pos-hs-transferpayment.css'
})
export class PosHsTransferpayment {

  banke: string = '';
  referenceNumber: string = '';

  getData() {
    return {
      paymentType: 'transfer',
      Banke: this.banke,
      ReferenceNumber: this.referenceNumber,
    };
  }

}
