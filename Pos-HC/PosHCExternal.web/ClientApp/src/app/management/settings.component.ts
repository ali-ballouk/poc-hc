import { TranslatePipe } from '../i18n/language';
import {
  Component,
  inject,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseAPI } from '../services/base.api';
@Component({
  standalone: true,
  imports: [TranslatePipe, FormsModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './settings.component.html',
})
export class SettingsComponent implements OnInit {
  api = inject(BaseAPI);
  settings: any;
  doctors: any[] = [];
  error = '';
  message = '';
  busy = false;
  fields = [
    { key: 'Name', label: 'Clinic name' },
    { key: 'Address', label: 'Address' },
    { key: 'Phone', label: 'Phone' },
    { key: 'LbpPerUsd', label: 'Lebanese lira per 1 USD', type: 'number' },
    { key: 'TaxRate', label: 'Applicable tax percentage', type: 'number' },
    { key: 'TaxRegistrationNumber', label: 'Tax registration number' },
  ];
  ngOnInit() {
    this.api.get<any[]>('api/doctor/lookup').subscribe({
      next: (doctors) => (this.doctors = doctors),
      error: () => (this.error = 'Unable to load doctors.'),
    });
    this.api.get('api/clinic/settings').subscribe({
      next: (r) => (this.settings = r),
      error: () => (this.error = 'Unable to load settings.'),
    });
  }
  save() {
    if (this.busy) return;
    if (this.settings.SingleDoctorMode && !this.settings.DefaultDoctorId) {
      this.error = 'Choose an active doctor for single-doctor mode.';
      return;
    }
    this.busy = true;
    this.error = '';
    this.message = '';
    this.api.put('api/clinic/settings', this.settings).subscribe({
      next: (r) => {
        this.settings = r;
        this.busy = false;
        this.message = 'Clinic settings saved.';
      },
      error: (e) => {
        this.busy = false;
        this.error = e.error?.detail || 'Unable to save settings.';
      },
    });
  }
}
