import {
  Component,
  inject,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { BaseAPI } from '../services/base.api';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import { GridColumn } from '../ui-shared/components/generic-grid/generic-grid.models';
@Component({
  standalone: true,
  imports: [FormsModule, GenericGridComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './reports.component.html',
})
export class ReportsComponent implements OnInit {
  api = inject(BaseAPI);
  http = inject(HttpClient);
  from = new Date().toISOString().slice(0, 8) + '01';
  to = new Date().toISOString().slice(0, 10);
  report: any;
  error = '';
  busy = false;
  sections = [
    { key: 'Summary', label: 'Financial summary' },
    { key: 'ByDoctor', label: 'Doctor billing' },
    { key: 'ByService', label: 'Service sales' },
    { key: 'ByPaymentMethod', label: 'Collections by payment method' },
  ];
  ngOnInit() {
    this.load();
  }
  columns(rows: any[]): GridColumn<any>[] {
    return Object.keys(rows?.[0] || {}).map((key) => ({
      key,
      header: key.replace(/([a-z])([A-Z])/g, '$1 $2'),
    }));
  }
  load() {
    this.busy = true;
    this.error = '';
    const end = new Date(this.to + 'T00:00:00');
    end.setDate(end.getDate() + 1);
    this.api
      .get(
        'api/reports?from=' +
          encodeURIComponent(new Date(this.from + 'T00:00:00').toISOString()) +
          '&to=' +
          encodeURIComponent(end.toISOString()),
      )
      .subscribe({
        next: (r) => {
          this.report = r;
          this.busy = false;
        },
        error: (e) => {
          this.busy = false;
          this.error = e.error?.detail || 'Unable to generate report.';
        },
      });
  }
  download(blob: Blob, name: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = name;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 60000);
  }
  csv(section: string) {
    const rows = this.report[section];
    if (!rows?.length) return;
    const keys = Object.keys(rows[0]);
    const cell = (v: any) =>
      '"' +
      String(v ?? '')
        .replace(/^[=+@-]/, "'$&")
        .replace(/"/g, '""') +
      '"';
    this.download(
      new Blob(
        [
          '\uFEFF' +
            [
              keys.map(cell).join(','),
              ...rows.map((r: any) => keys.map((k) => cell(r[k])).join(',')),
            ].join('\r\n'),
        ],
        { type: 'text/csv;charset=utf-8' },
      ),
      section + '.csv',
    );
  }
  exportData() {
    this.http.get('/api/reports/export', { responseType: 'blob' }).subscribe({
      next: (b) => this.download(b, 'Clinic-data.json'),
      error: () => (this.error = 'Unable to export clinic data.'),
    });
  }
}
