import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-pos-hs-cardpayment',
  imports: [
    FormsModule
  ],
  standalone: true,
  templateUrl: './pos-hs-cardpayment.html',
  styleUrl: './pos-hs-cardpayment.css'
})
export class PosHsCardpayment {
  cardNumberLast4: string = '';
  expiryDate: string = '';
  token: string = '';


  getData() {
    return {
      paymentType: 'card',
      CardNumberLast4: this.cardNumberLast4,
      expiryDate: this.expiryDate,
      Token: this.token
    };
  }
}
