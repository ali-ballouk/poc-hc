import { TranslatePipe } from '../../i18n/language';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-cashpayment',
  imports: [TranslatePipe, FormsModule],
  standalone: true,
  templateUrl: './pos-hs-cashpayment.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './pos-hs-cashpayment.css',
})
export class PosHsCashpayment {
  cashDrawerId: string = '';

  getData() {
    return {
      paymentType: 'cash',
      CashDrawerId: this.cashDrawerId,
    };
  }
}
