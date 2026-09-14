import {
  Component,
  inject,
  OnDestroy,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { TranslatePipe } from '../i18n/language';
import { BaseAPI } from '../services/base.api';
import { DIALOG_DATA } from '../services/dialog-ref';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridAction,
  GridColumn,
  GridPageChange,
  GridSort,
  GridPageResult,
} from '../ui-shared/components/generic-grid/generic-grid.models';
import { PatientVisit } from './patient-visit.models';
import { VisitNotesComponent } from './visit-notes.component';

@Component({
  standalone: true,
  imports: [TranslatePipe, GenericGridComponent],
  templateUrl: './patient-visit-history.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class PatientVisitHistoryComponent implements OnInit, OnDestroy {
  readonly data = inject(DIALOG_DATA) as {
    patientId: string;
    patientName?: string;
  };
  private readonly api = inject(BaseAPI);
  private readonly dialog = inject(DialogService);
  private request?: Subscription;
  private dialogs = new Subscription();
  rows: PatientVisit[] = [];
  sort?: GridSort<any>;
  page = 1;
  pageSize = 20;
  total = 0;
  loading = false;
  error = '';
  rowId = (row: PatientVisit) => row.Id;
  columns: GridColumn<PatientVisit>[] = [
    { key: 'VisitedAt', header: 'Visit date', type: 'date', widthPx: 140 },
    { key: 'Number', header: 'Invoice', widthPx: 110 },
    { key: 'DoctorName', header: 'Doctor', widthPx: 170 },
    { key: 'Status', header: 'Status', widthPx: 100 },
    { key: 'VisitDescription', header: 'Visit description', widthPx: 240 },
    { key: 'Diagnosis', header: 'Diagnosis', widthPx: 240 },
  ];
  actions: GridAction<PatientVisit>[] = [
    { label: 'View / edit notes', handler: (row) => this.open(row) },
  ];
  ngOnInit() {
    this.load();
  }
  ngOnDestroy() {
    this.request?.unsubscribe();
    this.dialogs.unsubscribe();
  }
  load() {
    this.request?.unsubscribe();
    this.loading = true;
    this.error = '';
    this.request = this.api
      .get<
        GridPageResult<PatientVisit>
      >(`api/clinic/patients/${this.data.patientId}/visits?page=${this.page}&pageSize=${this.pageSize}${this.sortQuery()}`)
      .subscribe({
        next: (result) => {
          this.rows = result.Items;
          this.total = result.Total;
          this.page = result.Page;
          this.pageSize = result.PageSize;
          this.loading = false;
        },
        error: (error) => {
          this.rows = [];
          this.error = error.error?.detail || 'Unable to load patient visits.';
          this.loading = false;
        },
      });
  }
  sortQuery() {
    return this.sort
      ? '&sortBy=' +
          encodeURIComponent(String(this.sort.columnKey)) +
          '&sortDirection=' +
          this.sort.direction
      : '';
  }
  onSortChange(sort: GridSort<any>) {
    this.sort = sort;
    this.page = 1;
    this.load();
  }
  onPageChange(event: GridPageChange) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
  }
  open(visit: PatientVisit) {
    this.dialogs.add(
      this.dialog
        .openComponent(
          VisitNotesComponent,
          'Visit notes (optional)',
          { patientId: this.data.patientId, visit },
          { nested: true },
        )
        .afterClosed()
        .subscribe((result) => {
          if (result) this.load();
        }),
    );
  }
}
