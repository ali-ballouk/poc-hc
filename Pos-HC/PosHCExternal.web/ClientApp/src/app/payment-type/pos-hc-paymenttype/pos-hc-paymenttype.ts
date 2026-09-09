import {
  Component,
  OnInit,
  Output,
  Input,
  EventEmitter,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { GenericSelectorComponent } from '../../ui-shared/components/generic-selector/generic-selector.component';

import { BaseAPI } from '../../services/base.api';

@Component({
  selector: 'pos-hs-paymenttype-selector',
  standalone: true,
  imports: [GenericSelectorComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './paymenttype-selector.component.html',
})
export class PaymentTypeSelectorComponent implements OnInit {
  paymenttypes = signal<any[]>([]);

  @Input() selectedPaymentTypeId: number | null = null;

  @Output() selectionChange = new EventEmitter<number | null>();

  @Output() selectedPaymentTypeIdChange = new EventEmitter<number | null>();

  constructor(private api: BaseAPI) {}

  ngOnInit(): void {
    this.api.get<any[]>('api/paymenttype/lookup').subscribe({
      next: (res) => {
        this.paymenttypes.set(res);
      },
      error: (err) => console.error('Error loading payment type', err),
    });
  }

  onSelectionChange(value: number | null) {
    this.selectedPaymentTypeId = value;
    this.selectedPaymentTypeIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
