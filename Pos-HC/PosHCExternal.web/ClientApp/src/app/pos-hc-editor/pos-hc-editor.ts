import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DoctorSelectorComponent } from '../doctors/doctor-selector/doctor-selector';
import { PatientSelectorComponent } from '../patients/patient-selector/patient-selector';
import { VisitItemsComponent } from '../visit-item/visit-item-component/visit-item-component';
import { BaseAPI } from '../services/base.api';
import { MatInputModule } from '@angular/material/input';
import { DialogService } from '../services/pos-hs-dialog.service';
import { PosHsPayment } from '../payment/pos-hs-payment/pos-hs-payment';  
interface Invoice {
  DoctorId: string;
  PatientId: string,
  Discount: Number,
  Items: any[]
}

type PaymentType = 'Cash' | 'Card' | 'Transfer' | 'OnAccount';

@Component({
  selector: 'pos-hc-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, PatientSelectorComponent, DoctorSelectorComponent, VisitItemsComponent, MatInputModule],
  templateUrl: './pos-hc-editor.html'
})
export class PosHcEditor {

 
  selectedPatientId: string | null = null;
  selectedDoctorId: string | null = null;

  @ViewChild(DoctorSelectorComponent) doctorSelector!: DoctorSelectorComponent;

  @ViewChild(VisitItemsComponent) visitItemsComponent!: VisitItemsComponent;

  doctorFee: number = 0;

  selectedItemId: string | null = null;
  qty = 1;
  discount = 0;
  paymentType: PaymentType = 'Cash';
  constructor(private api: BaseAPI, private dialog: DialogService) { }
  ngOnInit() {}
  onDoctorSelected(value: any) {
    this.doctorFee = this.doctorSelector.getSelectedDoctorFee();
    this.selectedDoctorId = value;
  }

  onPatientSelected(value: any) {
    this.selectedPatientId = value;
  }

  subtotal = 0;

  total() { return this.doctorFee + this.subtotal - this.discount; }

  onItemUpdated(totalItems: number) {

    this.subtotal = totalItems;
  }

  submit() {
    const payload = {
      DoctorId: this.selectedDoctorId,
      PatientId: this.selectedPatientId,
      Discount: this.discount,
      Items: this.visitItemsComponent.getVisitItems()
    };
    this.api.post<Invoice>('api/invoice', payload).subscribe({
      next: (res) => {
        console.log('Submitted successfully', res);
      },
      error: (err) => console.error('Error loading patients', err)
    });
  }

  pay() {

    this.dialog.openComponent(PosHsPayment, 'Payment', { invoiceId: 123 });

  }

  clear() {
    this.selectedPatientId = '';
    this.selectedDoctorId = '';
    this.doctorFee = 0;
    this.visitItemsComponent.clearItems();
  }

}
