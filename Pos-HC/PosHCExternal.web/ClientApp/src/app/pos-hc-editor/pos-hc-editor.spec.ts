import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { PosHcEditor } from './pos-hc-editor';

describe('PosHcEditor shared selectors', () => {
  it('updates the doctor fee, supports the shared grid within a form and clears every selector', async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcEditor],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(PosHcEditor);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http
      .expectOne('/api/clinic/settings')
      .flush({ LbpPerUsd: 89500, TaxRate: 0, ExchangeRateConfirmed: true });
    http
      .expectOne('/api/doctor/lookup')
      .flush([{ Id: 'doctor-1', FullName: 'Test Doctor', Fee: 35 }]);
    http
      .expectOne('/api/patient/lookup')
      .flush([{ Id: 'patient-1', FullName: 'Test Patient' }]);
    http.expectOne('/api/catalogtitem').flush([]);
    fixture.detectChanges();
    for (const selector of fixture.nativeElement.querySelectorAll(
      'pos-hs-patient-selector, pos-hs-doctor-selector',
    )) {
      selector.querySelector('.dropdown-toggle').click();
      fixture.detectChanges();
      selector.querySelector('[role="option"]').click();
      fixture.detectChanges();
    }
    expect(fixture.componentInstance.selectedDoctorId).toBe('doctor-1');
    expect(fixture.componentInstance.selectedPatientId).toBe('patient-1');
    expect(fixture.componentInstance.total()).toBe(35);
    fixture.componentInstance.clear();
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector(
        'pos-hs-doctor-selector .selector-text',
      ).textContent,
    ).toBe('Choose doctor');
    expect(
      fixture.nativeElement.querySelector(
        'pos-hs-patient-selector .selector-text',
      ).textContent,
    ).toBe('Choose patient');
    expect(fixture.componentInstance.total()).toBe(0);
    http.verify();
  });
});
