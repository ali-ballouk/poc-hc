import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '../../services/dialog-ref';

import { PosHcDialogShell } from './pos-hc-dialog-shell';

@Component({ changeDetection: ChangeDetectionStrategy.Eager, template: '' })
class TestDialogContent {}

describe('PosHcDialogShell', () => {
  let component: PosHcDialogShell;
  let fixture: ComponentFixture<PosHcDialogShell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosHcDialogShell],
      providers: [
        {
          provide: DIALOG_DATA,
          useValue: { component: TestDialogContent, title: 'Test dialog' },
        },
        { provide: DialogRef, useValue: { close: jasmine.createSpy('close') } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PosHcDialogShell);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
