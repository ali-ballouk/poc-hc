import { Component, OnInit, Output, Input, EventEmitter, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BaseAPI } from '../../services/base.api';

import { MatInputModule } from '@angular/material/input';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';

import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'pos-hs-doctor-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatInputModule],
  templateUrl: './doctor-selector.component.html'
})
export class DoctorSelectorComponent implements OnInit {
  doctors = signal<any[]>([]);

  @Input() selectedDoctorId: string | null = null;

  @Output() selectionChange = new EventEmitter<MatSelectChange>();

  @Output() selectedDoctorIdChange = new EventEmitter<string | null>();

  constructor(private api: BaseAPI) { }

  ngOnInit(): void {
    this.api.get<any[]>('api/doctors/lookup').subscribe({
      next: (res) => {
        this.doctors.set(res)
      },
      error: (err) => console.error('Error loading doctors', err)
    });
  }

  getSelectedDoctorFee(): number  {
    if (!this.selectedDoctorId) return 0;
    const doctor = this.doctors().find(d => d.Id === this.selectedDoctorId);
    return doctor.Fee;
  }



  onSelectionChange(event: MatSelectChange) {
    let value = event.value;
    this.selectedDoctorId = value;
    this.selectedDoctorIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
