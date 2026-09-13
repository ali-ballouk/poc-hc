import { TranslatePipe } from '../../i18n/language';
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
  selector: 'pos-hs-doctor-selector',
  standalone: true,
  imports: [TranslatePipe, GenericSelectorComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './doctor-selector.component.html',
})
export class DoctorSelectorComponent implements OnInit {
  doctors = signal<any[]>([]);

  @Input() selectedDoctorId: string | null = null;
  @Input() disabled = false;
  @Output() doctorsLoaded = new EventEmitter<any[]>();

  @Output() selectionChange = new EventEmitter<string | null>();

  @Output() selectedDoctorIdChange = new EventEmitter<string | null>();

  constructor(private api: BaseAPI) {}

  ngOnInit(): void {
    this.api.get<any[]>('api/doctor/lookup').subscribe({
      next: (res) => {
        this.doctors.set(res);
        this.doctorsLoaded.emit(res);
      },
      error: (err) => console.error('Error loading doctors', err),
    });
  }

  getSelectedDoctorFee(): number {
    if (!this.selectedDoctorId) return 0;
    const doctor = this.doctors().find((d) => d.Id === this.selectedDoctorId);
    return doctor?.Fee ?? 0;
  }

  onSelectionChange(value: string | null) {
    this.selectedDoctorId = value;
    this.selectedDoctorIdChange.emit(value);
    this.selectionChange.emit(value);
  }
}
