import { Component, OnInit, Output, Input, EventEmitter, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BaseAPI } from '../../services/base.api';

import { MatInputModule } from '@angular/material/input';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';

import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'pos-hs-paymenttype-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatInputModule],
  templateUrl: './paymenttype-selector.component.html'
})
export class PaymentTypeSelectorComponent implements OnInit {
  paymenttypes = signal<any[]>([]);

  @Input() selectedPaymentTypeId: Number | null = 0;

  @Output() selectionChange = new EventEmitter<MatSelectChange>();

  @Output() selectedPaymentTypeIdChange = new EventEmitter<string | null>();

  constructor(private api: BaseAPI) { }

  ngOnInit(): void {
    this.api.get<any[]>('api/paymenttype/lookup').subscribe({
      next: (res) => {
        this.paymenttypes.set(res)
      },
      error: (err) => console.error('Error loading payment type', err)
    });
  }

  
  onSelectionChange(event: MatSelectChange) {
    let value = event.value;
    this.selectedPaymentTypeId = value;
    this.selectedPaymentTypeIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
