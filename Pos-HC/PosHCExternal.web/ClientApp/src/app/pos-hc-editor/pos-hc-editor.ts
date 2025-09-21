import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Patient { id: string; fullName: string; }
interface Doctor { id: string; fullName: string; visitPrice: number; }
interface CatalogItem { id: string; name: string; unitPrice: number; }
interface VisitItem { catalogItemId: string; quantity: number; overrideUnitPrice?: number | null; }
type PaymentType = 'Cash' | 'Card' | 'Transfer' | 'OnAccount';

@Component({
  selector: 'pos-hc-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pos-hc-editor.html',
  styleUrls: ['./pos-hc-editor.css']
})
export class PosHcEditor {

  patients: Patient[] = [];
  doctors: Doctor[] = [];
  catalog: CatalogItem[] = [];
  catalogMap = new Map<string, CatalogItem>();

  patientId = '';
  doctorId = '';
  doctorFee = 0;

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

  setVisitPrice() {
    const doc = this.doctors.find(d => d.id === this.doctorId);
    this.doctorFee = doc?.visitPrice ?? 0;
  }

  addItem() {
    if (!this.selectedItemId || this.qty < 1) return;
    this.items.push({ catalogItemId: this.selectedItemId, quantity: this.qty });
    this.selectedItemId = null; this.qty = 1;
  }

  remove(i: number) { this.items.splice(i, 1); }

  unitPrice(it: VisitItem) {
    return it.overrideUnitPrice ?? (this.catalogMap.get(it.catalogItemId)?.unitPrice ?? 0);
  }

  rowTotal(it: VisitItem) { return this.unitPrice(it) * it.quantity; }

  subtotal() { return this.items.reduce((s, i) => s + this.rowTotal(i), 0); }

  total() { return this.doctorFee + this.subtotal() - this.discount; }

  submit() {
    const payload = {
      patientId: this.patientId,
      doctorId: this.doctorId,
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
}
