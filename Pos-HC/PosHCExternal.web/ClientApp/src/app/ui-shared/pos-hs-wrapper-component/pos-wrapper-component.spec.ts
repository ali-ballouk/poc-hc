import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosWrapperComponent } from './pos-hs-wrapper-component';

describe('PosWrapperComponent', () => {
  let component: PosWrapperComponent;
  let fixture: ComponentFixture<PosWrapperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosWrapperComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosWrapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
