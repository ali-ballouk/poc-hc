import {
  Component,
  inject,
  DestroyRef,
  ChangeDetectionStrategy,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../i18n/language';
import { DIALOG_DATA, DialogRef } from '../services/dialog-ref';
import { BaseAPI } from '../services/base.api';
import { PatientVisit } from './patient-visit.models';

@Component({
  standalone: true,
  imports: [TranslatePipe, FormsModule, DatePipe],
  templateUrl: './visit-notes.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class VisitNotesComponent {
  readonly data = inject(DIALOG_DATA) as {
    patientId: string;
    visit: PatientVisit;
  };
  readonly ref = inject(DialogRef);
  private readonly api = inject(BaseAPI);
  private readonly destroyRef = inject(DestroyRef);
  description = this.data.visit.VisitDescription ?? '';
  diagnosis = this.data.visit.Diagnosis ?? '';
  busy = false;
  error = '';
  save() {
    if (this.busy) return;
    this.busy = true;
    this.error = '';
    this.api
      .put<PatientVisit>(
        `api/patient/${this.data.patientId}/visits/${this.data.visit.Id}`,
        {
          VisitDescription: this.description.trim() || null,
          Diagnosis: this.diagnosis.trim() || null,
        },
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (visit) => {
          this.busy = false;
          this.ref.close(visit);
        },
        error: (error) => {
          this.busy = false;
          this.error = error.error?.detail || 'Unable to save visit notes.';
        },
      });
  }
}
