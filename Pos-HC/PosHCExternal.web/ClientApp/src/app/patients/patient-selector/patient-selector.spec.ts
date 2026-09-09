import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { PatientSelectorComponent } from './patient-selector';

describe('PatientSelectorComponent', () => {
  let component: PatientSelectorComponent;
  let fixture: ComponentFixture<PatientSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientSelectorComponent],
      providers: [provideHttpClient(withXhr()), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
