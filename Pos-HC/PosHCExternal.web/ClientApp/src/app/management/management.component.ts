import { TranslatePipe } from '../i18n/language';
import {
  Component,
  inject,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { BaseAPI } from '../services/base.api';
import { DialogService } from '../services/pos-hs-dialog.service';
import { GenericGridComponent } from '../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridAction,
  GridPageChange,
  GridSort,
  GridPageResult,
} from '../ui-shared/components/generic-grid/generic-grid.models';
import { AuthService } from './auth.service';
import { modules, ModuleDefinition, amount } from './module-definitions';
import { RecordFormComponent } from './record-form.component';
import { PatientVisitHistoryComponent } from './patient-visit-history.component';
@Component({
  standalone: true,
  imports: [TranslatePipe, FormsModule, GenericGridComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './management.component.html',
})
export class ManagementComponent implements OnInit, OnDestroy {
  api = inject(BaseAPI);
  auth = inject(AuthService);
  dialog = inject(DialogService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  key = 'patients';
  definition: ModuleDefinition = modules['patients'];
  rows: any[] = [];
  actions: GridAction<any>[] = [];
  sort?: GridSort<any>;
  page = 1;
  pageSize = 20;
  readonly pageSizeOptions = [10, 20, 50, 100];
  total = 0;
  search = '';
  loading = false;
  error = '';
  message = '';
  subscription?: Subscription;
  request?: Subscription;
  lookups = new Subscription();
  rowId = (row: any) => row.Id;
  ngOnInit() {
    this.subscription = this.route.paramMap.subscribe((params) => {
      this.request?.unsubscribe();
      this.lookups.unsubscribe();
      this.key = params.get('module') || 'patients';
      this.definition = modules[this.key] || modules['patients'];
      this.page = 1;
      this.total = 0;
      this.rows = [];
      this.search = '';
      this.message = '';
      if (!this.auth.can(...this.definition.roles)) {
        this.rows = [];
        this.error = 'You do not have permission to access this module.';
        return;
      }
      this.sort = undefined;
      this.configureActions();
      this.load();
    });
  }
  ngOnDestroy() {
    this.subscription?.unsubscribe();
    this.request?.unsubscribe();
    this.lookups.unsubscribe();
  }
  canWrite() {
    return this.auth.can(...this.definition.writers);
  }
  configureActions() {
    this.actions = [];
    if (this.key === 'patients') {
      this.actions.push({
        label: 'Visit history',
        handler: (row) =>
          this.dialog.openComponent(
            PatientVisitHistoryComponent,
            'Patient visit history',
            {
              patientId: row.Id,
              patientName: `${row.FirstName} ${row.LastName}`,
            },
          ),
      });
    }
    if (this.canWrite() && !this.definition.createOnly)
      this.actions.push({ label: 'Edit', handler: (row) => this.edit(row) });
    if (
      this.key === 'patients' &&
      this.auth.can('Administrator', 'Receptionist')
    )
      this.actions.push({
        label: 'Billing history',
        handler: (row) =>
          this.router.navigate(['/billing'], {
            queryParams: { patientId: row.Id },
          }),
      });
    if (this.key === 'staff')
      this.actions.push({
        label: 'Password reset',
        handler: (row) =>
          this.api
            .post<any>('api/clinic/staff/' + row.Id + '/reset', {})
            .subscribe({
              next: (r) =>
                (this.message =
                  'Reset token for ' +
                  row.Username +
                  ' (valid 30 minutes): ' +
                  r.Token),
              error: (e) =>
                (this.error = e.error?.detail || 'Unable to issue reset.'),
            }),
      });
    if (this.key === 'shifts') {
      this.actions.push(
        {
          label: 'Close shift',
          handler: (row) => this.shiftAction(row, 'close'),
        },
        {
          label: 'Cash movement',
          handler: (row) => this.shiftAction(row, 'movements'),
        },
      );
    }
  }
  load() {
    this.request?.unsubscribe();
    this.lookups.unsubscribe();
    this.lookups = new Subscription();
    this.loading = true;
    this.error = '';
    const key = this.key;
    this.request = this.api
      .get<
        GridPageResult<any>
      >('api/clinic/' + key + '?page=' + this.page + '&pageSize=' + this.pageSize + '&search=' + encodeURIComponent(this.search) + this.sortQuery())
      .subscribe({
        next: (result) => {
          this.rows = result.Items;
          this.total = result.Total;
          this.page = result.Page;
          this.pageSize = result.PageSize;
          this.loading = false;
          if (key === 'appointments' || key === 'availability') {
            this.lookups.add(
              this.api.get<any[]>('api/doctor/lookup').subscribe({
                next: (ds) => {
                  this.rows = this.rows.map((r) => ({
                    ...r,
                    DoctorName:
                      ds.find((d) => d.Id === r.DoctorId)?.FullName ||
                      r.DoctorId,
                  }));
                },
                error: () => (this.error = 'Unable to load records.'),
              }),
            );
            if (key === 'appointments')
              this.lookups.add(
                this.api.get<any[]>('api/patient/lookup').subscribe({
                  next: (ps) => {
                    this.rows = this.rows.map((r) => ({
                      ...r,
                      PatientName:
                        ps.find((p) => p.Id === r.PatientId)?.FullName ||
                        r.PatientId,
                    }));
                  },
                  error: () => (this.error = 'Unable to load records.'),
                }),
              );
          }
        },
        error: (e) => {
          this.loading = false;
          this.rows = [];
          this.error = e.error?.detail || 'Unable to load records.';
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
  edit(row?: any) {
    this.dialog
      .openComponent(
        RecordFormComponent,
        (row ? 'Edit ' : 'Add ') + this.definition.title,
        {
          fields: this.definition.fields,
          value: row || this.definition.defaults,
          endpoint: 'api/clinic/' + this.key + (row ? '/' + row.Id : ''),
          method: row ? 'put' : 'post',
        },
      )
      .afterClosed()
      .subscribe((result) => {
        if (result) this.load();
      });
  }
  shiftAction(row: any, action: string) {
    if (row.ClosedAt) {
      this.error = 'This shift is already closed.';
      return;
    }
    this.dialog
      .openComponent(
        RecordFormComponent,
        action === 'close' ? 'Close cash shift' : 'Record cash movement',
        {
          fields: [
            {
              ...amount,
              label:
                action === 'close'
                  ? 'Counted cash'
                  : 'Amount (negative for withdrawal)',
            },
            ...(action === 'close'
              ? []
              : [{ key: 'Reason', label: 'Reason', required: true }]),
          ],
          value: { Amount: 0 },
          endpoint: 'api/clinic/shifts/' + row.Id + '/' + action,
        },
      )
      .afterClosed()
      .subscribe((result) => {
        if (result) this.load();
      });
  }
}
