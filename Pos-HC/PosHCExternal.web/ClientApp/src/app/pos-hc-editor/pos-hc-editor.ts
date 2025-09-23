import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DoctorSelectorComponent } from '../doctors/doctor-selector/doctor-selector';
import { PatientSelectorComponent } from '../patients/patient-selector/patient-selector';
import { VisitItemsComponent } from '../visit-item/visit-item-component/visit-item-component';
import { ISelectorApi } from '../interfaces/iselector.api';
import { MatSelectChange } from '@angular/material/select';



interface Patient { id: string; fullName: string; }
interface Doctor { id: string; fullName: string; visitPrice: number; }
interface CatalogItem { id: string; name: string; unitPrice: number; }
interface VisitItem { catalogItemId: string; quantity: number; overrideUnitPrice?: number | null; }
type PaymentType = 'Cash' | 'Card' | 'Transfer' | 'OnAccount';

@Component({
  selector: 'pos-hc-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, PatientSelectorComponent, DoctorSelectorComponent, VisitItemsComponent],
  templateUrl: './pos-hc-editor.html',
  styleUrls: ['./pos-hc-editor.css']
})
export class PosHcEditor {

  patients: Patient[] = [];
  doctors: Doctor[] = [];
  catalog: CatalogItem[] = [];
  catalogMap = new Map<string, CatalogItem>();
  selectedPatientId: string | null = null;
  selectedDoctorId: string | null = null;

  @ViewChild(DoctorSelectorComponent) doctorSelector!: DoctorSelectorComponent;

  @ViewChild(VisitItemsComponent) visitItemsComponent!: VisitItemsComponent;

  doctorFee : number  = 0;

  selectedItemId: string | null = null;
  qty = 1;
  items: VisitItem[] = [];

  discount = 0;
  paymentType: PaymentType = 'Cash';

  
  ngOnInit() {
    //this.api.patients().subscribe(p => { this.patients = p; this.patientId = p[0]?.id ?? ''; });
    //this.api.doctors().subscribe(d => { this.doctors = d; this.doctorId = d[0]?.id ?? ''; this.setVisitPrice(); });
    //this.api.catalog().subscribe(c => { this.catalog = c; c.forEach(x => this.catalogMap.set(x.id, x)); });
  }
  onDoctorSelected(event: MatSelectChange) {
    this.doctorFee = this.doctorSelector.getSelectedDoctorFee();
    this.selectedDoctorId = event.value;
  }
  
  onPatientSelected(event: MatSelectChange) {
    this.selectedPatientId = event.value;
  }




  rowTotal(it: VisitItem) { 0 }

  subtotal = 0;

  total() { return this.doctorFee + this.subtotal - this.discount; }

  onItemUpdated(totalItems: number) {
    
    this.subtotal = totalItems;
  }

  submit() {
    console.log(this.visitItemsComponent.getVisitItems())
    const payload = {
      patientId: this.selectedPatientId,
      doctorId: this.selectedDoctorId,
      discount: this.discount,
      items: this.items,
      payment: {
        paymentType: this.paymentType,
        amount: this.total()
      }
    };
    console.log('Submitting visit:', payload);
    // TODO: send to backend
    // this.api.createVisit(payload).subscribe(...)
  }

  clear() {
    this.selectedPatientId = '';
    this.selectedDoctorId = '';
    this.doctorFee = 0;
    this.visitItemsComponent.clearItems();
  }

}
