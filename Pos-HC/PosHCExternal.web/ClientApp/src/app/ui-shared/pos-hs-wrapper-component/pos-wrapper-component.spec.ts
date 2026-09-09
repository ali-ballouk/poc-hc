import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHsWrapperComponent } from './pos-hs-wrapper-component';

describe('PosHsWrapperComponent', () => {
  let component: PosHsWrapperComponent;
  let fixture: ComponentFixture<PosHsWrapperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHsWrapperComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PosHsWrapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
