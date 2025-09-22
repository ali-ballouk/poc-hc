import { Component, OnInit, Output, Input, EventEmitter, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BaseAPI } from '../../services/base.api';
import { ISelectorApi } from '../../interfaces/iselector.api';

import { MatInputModule } from '@angular/material/input';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';

import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'pos-hs-patient-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatInputModule],
  templateUrl: './patient-selector.component.html'
})
export class PatientSelectorComponent implements OnInit, ISelectorApi {
  patients = signal<any[]>([]);
  selectedPatientId: string = '';

  @Input() functionOnReady?: (api: ISelectorApi) => void;

  @Output() selectionChange = new EventEmitter<MatSelectChange>();
  constructor(private api: BaseAPI) { }

  ngOnInit(): void {
    this.api.get<any[]>('api/patient/lookup').subscribe({
      next: (res) => {
        this.patients.set(res)

        //if (this.functionOnReady) {
        //  this.functionOnReady(this);
        //}
      },
      error: (err) => console.error('Error loading patients', err)
    });
  }

  onPatientSelected(event: MatSelectChange) {
    this.selectionChange.emit(event);
  }

  getSelectedId(): string | null {
    return this.selectedPatientId;
  }
}
