import { TranslatePipe } from '../../i18n/language';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-onaccountpayment',
  standalone: true,
  imports: [TranslatePipe, FormsModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hs-onaccountpayment.html',
})
export class PosHsOnaccountpayment {
  accountId: string = '';

  getData() {
    return {
      paymentType: 'on-account',
      AccountId: this.accountId,
    };
  }
}
