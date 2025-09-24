import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-cashpayment',
  imports: [FormsModule],
  standalone: true,
  templateUrl: './pos-hs-cashpayment.html',
  styleUrl: './pos-hs-cashpayment.css'
})
export class PosHsCashpayment {
  cashDrawerId: string = '';

  getData() {
    return {
      paymentType: 'cash',
      CashDrawerId: this.cashDrawerId
    };
  }
}
