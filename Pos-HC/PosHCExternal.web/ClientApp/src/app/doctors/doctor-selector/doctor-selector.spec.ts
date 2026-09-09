import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { DoctorSelectorComponent } from './doctor-selector';

describe('DoctorSelectorComponent', () => {
  let component: DoctorSelectorComponent;
  let fixture: ComponentFixture<DoctorSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorSelectorComponent],
      providers: [provideHttpClient(withXhr()), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
