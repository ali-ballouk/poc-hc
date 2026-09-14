import { TranslatePipe, LocalNumberPipe } from '../i18n/language';
import {
  Component,
  ViewChild,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import { AuthService } from '../management/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { DoctorSelectorComponent } from '../doctors/doctor-selector/doctor-selector';
import { PatientSelectorComponent } from '../patients/patient-selector/patient-selector';
import { VisitItemsComponent } from '../visit-item/visit-item-component/visit-item-component';
import { BaseAPI } from '../services/base.api';
import { DialogService } from '../services/pos-hs-dialog.service';
import { PosHsPayment } from '../payment/pos-hs-payment/pos-hc-payment';
import { PatientVisitHistoryComponent } from '../management/patient-visit-history.component';
interface Invoice {
  DoctorId: string;
  PatientId: string;
  Discount: Number;
  Items: any[];
}

@Component({
  selector: 'pos-hc-editor',
  standalone: true,
  imports: [
    TranslatePipe,
    LocalNumberPipe,
    CommonModule,
    FormsModule,
    PatientSelectorComponent,
    DoctorSelectorComponent,
    VisitItemsComponent,
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-editor.html',
})
export class PosHcEditor {
  auth = inject(AuthService);
  currency = 'USD';
  draft = false;
  settings: any = { LbpPerUsd: 1, TaxRate: 0 };
  settingsLoaded = false;
  private doctors: any[] = [];
  visitDescription = '';
  diagnosis = '';
  saving = false;
  error = '';
  requestId = crypto.randomUUID();

  selectedPatientId: string | null = null;
  selectedDoctorId: string | null = null;
  invoiceResult: any | null = null;

  @ViewChild(DoctorSelectorComponent) doctorSelector!: DoctorSelectorComponent;

  @ViewChild(VisitItemsComponent) visitItemsComponent!: VisitItemsComponent;

  doctorFee: number = 0;

  selectedItemId: string | null = null;
  qty = 1;
  discount = 0;
  constructor(
    private api: BaseAPI,
    private dialog: DialogService,
  ) {}
  ngOnInit() {
    this.api.get('api/clinic/settings').subscribe({
      next: (r) => {
        this.settings = r;
        this.settingsLoaded = true;
        this.applyClinicDoctor();
      },
      error: () => (this.error = 'Unable to load clinic settings.'),
    });
  }
  onDoctorSelected(value: any) {
    this.doctorFee = this.doctorSelector.getSelectedDoctorFee();
    this.selectedDoctorId = value;
  }

  onDoctorsLoaded(doctors: any[]) {
    this.doctors = doctors;
    this.applyClinicDoctor();
  }
  private applyClinicDoctor() {
    if (!this.settings.SingleDoctorMode) return;
    const doctor = this.doctors.find(
      (d) => d.Id === this.settings.DefaultDoctorId,
    );
    this.selectedDoctorId = doctor?.Id ?? null;
    this.doctorFee = doctor?.Fee ?? 0;
  }
  openVisitHistory() {
    if (!this.selectedPatientId) return;
    this.dialog.openComponent(
      PatientVisitHistoryComponent,
      'Patient visit history',
      { patientId: this.selectedPatientId },
    );
  }

  onPatientSelected(value: any) {
    this.selectedPatientId = value;
  }

  subtotal = 0;

  converted(value: number) {
    const rate = this.currency === 'LBP' ? this.settings.LbpPerUsd : 1;
    const scale = this.currency === 'LBP' ? 1 : 100;
    return Math.round(value * rate * scale) / scale;
  }
  itemTotal() {
    return (
      this.visitItemsComponent?.rows.reduce(
        (sum, row) => sum + this.converted(row.unitPrice) * row.quantity,
        0,
      ) ?? this.converted(this.subtotal)
    );
  }
  tax() {
    const scale = this.currency === 'LBP' ? 1 : 100;
    return (
      Math.round(
        (((this.converted(this.doctorFee) + this.itemTotal() - this.discount) *
          this.settings.TaxRate) /
          100) *
          scale,
      ) / scale
    );
  }
  total() {
    return (
      this.converted(this.doctorFee) +
      this.itemTotal() -
      this.discount +
      this.tax()
    );
  }

  onItemUpdated(totalItems: number) {
    this.subtotal = totalItems;
  }

  submit() {
    if (this.saving || this.invoiceResult) return;
    if (!this.settingsLoaded) {
      this.error = 'Unable to load clinic settings.';
      return;
    }
    if (!this.selectedDoctorId || !this.selectedPatientId) {
      this.error = 'Select a patient and doctor.';
      return;
    }
    this.saving = true;
    this.error = '';
    const payload = {
      Currency: this.currency,
      Draft: this.draft,
      RequestId: this.requestId,
      DoctorId: this.selectedDoctorId,
      PatientId: this.selectedPatientId,
      VisitDescription: this.visitDescription.trim() || null,
      Diagnosis: this.diagnosis.trim() || null,
      Discount: this.discount,
      Items: this.visitItemsComponent.getVisitItems(),
    };
    this.api.post<Invoice>('api/invoice', payload).subscribe({
      next: (res) => {
        this.saving = false;
        this.invoiceResult = res;
      },
      error: (err) => {
        this.saving = false;
        this.error = err.error?.detail || 'Unable to create invoice.';
      },
    });
  }

  pay() {
    let params = {
      invoiceId: this.invoiceResult.InvoiceId,
    };
    this.dialog
      .openComponent(PosHsPayment, 'Record payment taken from patient', params)
      .afterClosed()
      .subscribe((result) => {
        if (result?.success) this.clear();
      });
  }

  clear() {
    this.selectedPatientId = '';
    this.selectedDoctorId = '';
    this.doctorFee = 0;
    this.applyClinicDoctor();
    this.visitDescription = '';
    this.diagnosis = '';
    this.discount = 0;
    this.visitItemsComponent.clearItems();
    this.invoiceResult = null;
    this.requestId = crypto.randomUUID();
    this.error = '';
    this.draft = false;
  }
}
