import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pos-hs-onaccountpayment',
  standalone: true,
  imports: [FormsModule], 
  templateUrl: './pos-hs-onaccountpayment.html'
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
