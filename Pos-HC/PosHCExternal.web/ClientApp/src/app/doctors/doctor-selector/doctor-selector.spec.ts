import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorSelector } from './doctor-selector';

describe('DoctorSelector', () => {
  let component: DoctorSelector;
  let fixture: ComponentFixture<DoctorSelector>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorSelector]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DoctorSelector);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
