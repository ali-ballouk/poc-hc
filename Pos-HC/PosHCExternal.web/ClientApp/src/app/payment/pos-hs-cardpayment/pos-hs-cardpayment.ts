import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-cardpayment',
  imports: [FormsModule],
  standalone: true,
  templateUrl: './pos-hs-cardpayment.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './pos-hs-cardpayment.css',
})
export class PosHsCardpayment {
  cardNumberLast4: string = '';
  expiryDate: string = '';
  token: string = '';

  getData() {
    return {
      paymentType: 'card',
      CardNumber: this.cardNumberLast4,
      Expiry: this.expiryDate,
      Token: this.token,
    };
  }
}
