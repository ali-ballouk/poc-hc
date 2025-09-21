import { Component, OnInit, Output, Input, EventEmitter, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BaseAPI } from '../../services/base.api';
import { ISelectorApi } from '../../interfaces/iselector.api';

@Component({
  selector: 'pos-hs-doctor-selector',
  standalone: true,                
  imports: [CommonModule, FormsModule], 
  templateUrl: './doctor-selector.component.html'
})
export class DoctorSelectorComponent implements OnInit , ISelectorApi {
  doctors = signal<any[]>([]);
  selectedDoctorId: string = '';

  @Input() functionOnReady?: (api: ISelectorApi) => void;

  @Output() selectedDoctorChange = new EventEmitter<string>();

  constructor(private api: BaseAPI) { }

  ngOnInit(): void {
    this.api.get<any[]>('api/doctor/lookup').subscribe({
      next: (res) => {
        this.doctors.set(res)

        //if (this.functionOnReady) {
        //  this.functionOnReady(this);
        //}
      },
      error: (err) => console.error('Error loading doctors', err)
    });
  }

  onChange() {
    this.selectedDoctorChange.emit(this.selectedDoctorId);
  }

  getSelectedId(): string | null {
    return this.selectedDoctorId;
  }
}
