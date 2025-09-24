import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosHcDialogShell } from './pos-hc-dialog-shell';

describe('PosHcDialogShell', () => {
  let component: PosHcDialogShell;
  let fixture: ComponentFixture<PosHcDialogShell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcDialogShell]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PosHcDialogShell);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
