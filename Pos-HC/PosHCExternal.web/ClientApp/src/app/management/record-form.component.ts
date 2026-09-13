import { TranslatePipe } from '../i18n/language';
import {
  Component,
  Inject,
  ChangeDetectionStrategy,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseAPI } from '../services/base.api';
import { DIALOG_DATA, DialogRef } from '../services/dialog-ref';
import { GenericSelectorComponent } from '../ui-shared/components/generic-selector/generic-selector.component';
import { Field } from './module-definitions';
export interface RecordFormData {
  fields: Field[];
  value?: Record<string, any>;
  endpoint: string;
  method?: 'post' | 'put';
  prepare?: (value: any) => any;
}
@Component({
  standalone: true,
  imports: [TranslatePipe, FormsModule, GenericSelectorComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './record-form.component.html',
})
export class RecordFormComponent implements OnInit {
  value: Record<string, any> = {};
  choices: Record<string, any[]> = {};
  busy = false;
  loadingDoctorMode = false;
  singleDoctorId: string | null = null;
  error = '';
  constructor(
    @Inject(DIALOG_DATA) public data: RecordFormData,
    public ref: DialogRef,
    private api: BaseAPI,
  ) {
    this.value = { ...data.value };
    for (const field of data.fields) {
      const v = this.value[field.key];
      if (v && field.type === 'datetime-local') {
        const d = new Date(v.endsWith('Z') ? v : v + 'Z');
        this.value[field.key] = new Date(
          d.getTime() - d.getTimezoneOffset() * 60000,
        )
          .toISOString()
          .slice(0, 16);
      }
      if (v && field.type === 'date') this.value[field.key] = v.slice(0, 10);
    }
  }
  ngOnInit() {
    if (
      this.data.fields.some((field) => field.key === 'DoctorId') &&
      this.data.method !== 'put' &&
      !this.data.value?.['Id']
    ) {
      this.loadingDoctorMode = true;
      this.api.get<any>('api/clinic/settings').subscribe({
        next: (settings) => {
          if (settings.SingleDoctorMode) {
            this.singleDoctorId = settings.DefaultDoctorId;
            this.value['DoctorId'] = settings.DefaultDoctorId;
          }
          this.loadingDoctorMode = false;
        },
        error: () => {
          this.error = 'Unable to load clinic settings.';
        },
      });
    }
    for (const field of this.data.fields)
      if (field.lookup)
        this.api.get<any[]>(field.lookup).subscribe({
          next: (rows) =>
            (this.choices[field.key] = rows.map((x) => ({
              ...x,
              Name: x.FullName || x.Name,
            }))),
          error: () =>
            (this.error =
              'Unable to load choices. Please close and try again.'),
        });
  }
  save() {
    if (this.busy || this.loadingDoctorMode) return;
    this.error = '';
    for (const field of this.data.fields)
      if (
        field.required &&
        (this.value[field.key] === null ||
          this.value[field.key] === undefined ||
          this.value[field.key] === '')
      ) {
        this.error = field.label + ' is required.';
        return;
      }
    const payload = { ...this.value };
    for (const field of this.data.fields) {
      if (field.type === 'datetime-local' && payload[field.key])
        payload[field.key] = new Date(payload[field.key]).toISOString();
      if (field.type === 'date' && !payload[field.key])
        payload[field.key] = null;
    }
    this.busy = true;
    const body = this.data.prepare ? this.data.prepare(payload) : payload;
    const request =
      this.data.method === 'put'
        ? this.api.put(this.data.endpoint, body)
        : this.api.post(this.data.endpoint, body);
    request.subscribe({
      next: (result) => {
        this.busy = false;
        this.ref.close(result);
      },
      error: (e) => {
        this.busy = false;
        this.error =
          e.error?.detail ||
          Object.values(e.error?.errors || {})
            .flat()
            .join(' ') ||
          'Unable to save. Please try again.';
      },
    });
  }
}
