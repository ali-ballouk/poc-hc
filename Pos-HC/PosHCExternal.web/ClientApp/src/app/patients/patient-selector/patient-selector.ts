import {
  Component,
  OnInit,
  Output,
  Input,
  EventEmitter,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { GenericSelectorComponent } from '../../ui-shared/components/generic-selector/generic-selector.component';

import { BaseAPI } from '../../services/base.api';

@Component({
  selector: 'pos-hs-patient-selector',
  standalone: true,
  imports: [GenericSelectorComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './patient-selector.component.html',
})
export class PatientSelectorComponent implements OnInit {
  patients = signal<any[]>([]);

  @Input() selectedPatientId: string | null = null;

  @Output() selectionChange = new EventEmitter<string | null>(); // define output call back

  @Output() selectedPatientIdChange = new EventEmitter<string | null>();

  constructor(private api: BaseAPI) {}

  ngOnInit(): void {
    this.api.get<any[]>('api/patient/lookup').subscribe({
      next: (res) => {
        this.patients.set(res);
      },
      error: (err) => console.error('Error loading patients', err),
    });
  }

  onSelectionChange(value: string | null) {
    this.selectedPatientId = value;
    this.selectedPatientIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
