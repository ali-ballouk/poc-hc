import { Component, OnInit, Output, Input, EventEmitter, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BaseAPI } from '../../services/base.api';

import { MatInputModule } from '@angular/material/input';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';

import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'pos-hs-patient-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatInputModule],
  templateUrl: './patient-selector.component.html'
})
export class PatientSelectorComponent implements OnInit {
  patients = signal<any[]>([]);

  @Input() selectedPatientId: string | null = null;

  @Output() selectionChange = new EventEmitter<MatSelectChange>(); // define output call back

  @Output() selectedPatientIdChange = new EventEmitter<string | null>();

  constructor(private api: BaseAPI) { }

  ngOnInit(): void {
    this.api.get<any[]>('api/patient/lookup').subscribe({
      next: (res) => {
        this.patients.set(res);
      },
      error: (err) => console.error('Error loading patients', err)
    });
  }

  onMatSelectionChange(event: MatSelectChange) {
    let value = event.value;
    this.selectedPatientId = value;
    this.selectedPatientIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
