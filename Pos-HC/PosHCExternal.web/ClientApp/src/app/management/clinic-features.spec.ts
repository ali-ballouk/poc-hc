import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { of } from 'rxjs';
import { SettingsComponent } from './settings.component';
import { PosHcEditor } from '../pos-hc-editor/pos-hc-editor';
import { PatientVisitHistoryComponent } from './patient-visit-history.component';
import { VisitNotesComponent } from './visit-notes.component';
import { RecordFormComponent } from './record-form.component';
import { DIALOG_DATA, DialogRef } from '../services/dialog-ref';
import { DialogService } from '../services/pos-hs-dialog.service';

describe('Single-doctor mode and patient visit notes', () => {
  const visit = {
    Id: 'visit-1',
    Number: 1001,
    VisitedAt: '2026-09-11T07:00:00Z',
    DoctorName: 'Clinic Doctor',
    Status: 'Issued',
    VisitDescription: null,
    Diagnosis: null,
    UpdatedAt: null,
    UpdatedBy: null,
  };
  let close: jasmine.Spy;
  let open: jasmine.Spy;
  beforeEach(() => {
    close = jasmine.createSpy('close');
    open = jasmine
      .createSpy('openComponent')
      .and.returnValue({ afterClosed: () => of({ saved: true }) });
    TestBed.configureTestingModule({
      imports: [
        SettingsComponent,
        PosHcEditor,
        PatientVisitHistoryComponent,
        VisitNotesComponent,
        RecordFormComponent,
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DialogRef, useValue: { close } },
        { provide: DialogService, useValue: { openComponent: open } },
        { provide: DIALOG_DATA, useValue: { patientId: 'patient-1', visit } },
      ],
    });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('requires a configured doctor before saving single-doctor mode', () => {
    const fixture = TestBed.createComponent(SettingsComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne('/api/doctor/lookup')
      .flush([{ Id: 'doctor-1', FullName: 'Clinic Doctor' }]);
    http
      .expectOne('/api/clinic/settings')
      .flush({
        Name: 'Clinic',
        LbpPerUsd: 1,
        SingleDoctorMode: false,
        DefaultDoctorId: null,
      });
    fixture.componentInstance.settings.SingleDoctorMode = true;
    fixture.componentInstance.save();
    expect(fixture.componentInstance.error).toContain(
      'Choose an active doctor',
    );
    http.expectNone((request) => request.method === 'PUT');
    fixture.componentInstance.settings.DefaultDoctorId = 'doctor-1';
    fixture.componentInstance.save();
    const save = http.expectOne('/api/clinic/settings');
    expect(save.request.body.DefaultDoctorId).toBe('doctor-1');
    save.flush(save.request.body);
    expect(fixture.componentInstance.message).toBe('Clinic settings saved.');
  });

  for (const settingsFirst of [true, false]) {
    it(`assigns and retains the configured doctor when ${settingsFirst ? 'settings' : 'doctors'} load first`, () => {
      const fixture = TestBed.createComponent(PosHcEditor);
      fixture.componentInstance.auth.user.set({
        Username: 'admin',
        Role: 'Administrator',
      });
      const http = TestBed.inject(HttpTestingController);
      fixture.detectChanges();
      const settings = http.expectOne('/api/clinic/settings');
      const doctors = http.expectOne('/api/doctor/lookup');
      const settingValue = {
        LbpPerUsd: 1,
        TaxRate: 0,
        SingleDoctorMode: true,
        DefaultDoctorId: 'doctor-1',
      };
      const doctorValue = [
        { Id: 'doctor-1', FullName: 'Clinic Doctor', Fee: 35 },
      ];
      if (settingsFirst) {
        settings.flush(settingValue);
        doctors.flush(doctorValue);
      } else {
        doctors.flush(doctorValue);
        settings.flush(settingValue);
      }
      http
        .expectOne('/api/patient/lookup')
        .flush([{ Id: 'patient-1', FullName: 'Patient' }]);
      http.expectOne('/api/catalogtitem').flush([]);
      fixture.detectChanges();
      expect(fixture.componentInstance.selectedDoctorId).toBe('doctor-1');
      expect(fixture.componentInstance.total()).toBe(35);
      expect(
        fixture.nativeElement.querySelector(
          'pos-hs-doctor-selector .dropdown-toggle',
        ).disabled,
      ).toBeTrue();
      fixture.componentInstance.selectedPatientId = 'patient-1';
      fixture.componentInstance.visitDescription = ' Follow-up ';
      fixture.componentInstance.diagnosis = ' Optional diagnosis ';
      fixture.componentInstance.submit();
      const save = http.expectOne('/api/invoice');
      expect(save.request.body.VisitDescription).toBe('Follow-up');
      expect(save.request.body.Diagnosis).toBe('Optional diagnosis');
      save.flush({ InvoiceId: 'visit-1', Total: 35 });
      fixture.componentInstance.clear();
      expect(fixture.componentInstance.selectedDoctorId).toBe('doctor-1');
      expect(fixture.componentInstance.doctorFee).toBe(35);
      expect(fixture.componentInstance.visitDescription).toBe('');
      expect(fixture.componentInstance.diagnosis).toBe('');
    });
  }

  it('allows both notes to remain blank and keeps failed edits open', () => {
    const fixture = TestBed.createComponent(VisitNotesComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    fixture.componentInstance.save();
    const request = http.expectOne(
      '/api/clinic/patients/patient-1/visits/visit-1',
    );
    expect(request.request.body).toEqual({
      VisitDescription: null,
      Diagnosis: null,
    });
    request.flush(
      { detail: 'Temporary error' },
      { status: 500, statusText: 'Error' },
    );
    expect(close).not.toHaveBeenCalled();
    expect(fixture.componentInstance.error).toBe('Temporary error');
    fixture.componentInstance.save();
    http
      .expectOne('/api/clinic/patients/patient-1/visits/visit-1')
      .flush(visit);
    expect(close).toHaveBeenCalledWith(visit);
  });

  it('loads backend patient history and refreshes after editing a visit', () => {
    const fixture = TestBed.createComponent(PatientVisitHistoryComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne('/api/clinic/patients/patient-1/visits?page=1&pageSize=20')
      .flush({ Items: [visit], Total: 21, Page: 1, PageSize: 20 });
    fixture.componentInstance.onPageChange({ pageIndex: 1, pageSize: 20 });
    http
      .expectOne('/api/clinic/patients/patient-1/visits?page=2&pageSize=20')
      .flush({ Items: [visit], Total: 21, Page: 2, PageSize: 20 });
    fixture.componentInstance.open(visit);
    expect(open.calls.mostRecent().args[0]).toBe(VisitNotesComponent);
    expect(open.calls.mostRecent().args[3]).toEqual({ nested: true });
    http
      .expectOne('/api/clinic/patients/patient-1/visits?page=2&pageSize=20')
      .flush({ Items: [visit], Total: 21, Page: 2, PageSize: 20 });
    expect(fixture.componentInstance.page).toBe(2);
  });

  it('defaults new appointment forms to the clinic doctor', () => {
    TestBed.overrideProvider(DIALOG_DATA, {
      useValue: {
        fields: [
          {
            key: 'DoctorId',
            label: 'Doctor',
            type: 'select',
            required: true,
            lookup: 'api/doctor/lookup',
          },
        ],
        endpoint: 'api/clinic/appointments',
        value: {},
      },
    });
    const fixture = TestBed.createComponent(RecordFormComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne('/api/clinic/settings')
      .flush({ SingleDoctorMode: true, DefaultDoctorId: 'doctor-1' });
    http
      .expectOne('/api/doctor/lookup')
      .flush([{ Id: 'doctor-1', FullName: 'Clinic Doctor' }]);
    fixture.detectChanges();
    expect(fixture.componentInstance.value['DoctorId']).toBe('doctor-1');
    expect(
      fixture.nativeElement.querySelector('.dropdown-toggle').disabled,
    ).toBeTrue();
  });
});
