import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { RecordFormComponent } from './record-form.component';
import { DialogRef, DIALOG_DATA } from '../services/dialog-ref';
import { AuthService } from './auth.service';
describe('Clinic module workflows', () => {
  it('requires a patient selector and keeps failed edits open', async () => {
    const close = jasmine.createSpy('close');
    await TestBed.configureTestingModule({
      imports: [RecordFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DialogRef, useValue: { close } },
        {
          provide: DIALOG_DATA,
          useValue: {
            fields: [
              {
                key: 'PatientId',
                label: 'Patient',
                required: true,
                type: 'select',
                options: [],
              },
            ],
            endpoint: 'api/appointments',
            value: {},
          },
        },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(RecordFormComponent);
    fixture.detectChanges();
    fixture.componentInstance.save();
    expect(fixture.componentInstance.error).toContain('Patient is required');
    expect(close).not.toHaveBeenCalled();
    fixture.componentInstance.value['PatientId'] = 'patient-1';
    fixture.componentInstance.save();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/appointments')
      .flush(
        { detail: 'Doctor is already booked' },
        { status: 400, statusText: 'Bad request' },
      );
    expect(fixture.componentInstance.error).toBe('Doctor is already booked');
    expect(close).not.toHaveBeenCalled();
    expect(fixture.componentInstance.busy).toBeFalse();
  });
  it('clears the user after a successful logout', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const auth = TestBed.inject(AuthService);
    const http = TestBed.inject(HttpTestingController);
    auth.user.set({ Username: 'admin', Role: 'Administrator' });
    expect(auth.can('Administrator')).toBeTrue();
    auth.logout();
    http.expectOne('/api/auth/logout').flush(null);
    http
      .expectOne('/api/auth/session')
      .flush({ User: null, SetupRequired: false });
    expect(auth.user()).toBeNull();
    http.verify();
  });
});
