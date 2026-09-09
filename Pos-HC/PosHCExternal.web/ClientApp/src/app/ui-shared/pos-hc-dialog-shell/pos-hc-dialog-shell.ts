import {
  Component,
  Inject,
  Injector,
  Type,
  ChangeDetectionStrategy,
} from '@angular/core';
import { NgComponentOutlet } from '@angular/common';
import { DIALOG_DATA, DialogRef } from '../../services/dialog-ref';

export interface DialogShellData {
  title?: string;
  component: Type<any>;
  data?: any;
}

@Component({
  selector: 'app-dialog-shell',
  standalone: true,
  imports: [NgComponentOutlet],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-dialog-shell.html',
})
export class PosHcDialogShell {
  readonly childInjector: Injector;
  constructor(
    @Inject(DIALOG_DATA) public data: DialogShellData,
    private dialogRef: DialogRef,
    injector: Injector,
  ) {
    this.childInjector = Injector.create({
      providers: [
        { provide: DIALOG_DATA, useValue: data.data },
        { provide: DialogRef, useValue: dialogRef },
      ],
      parent: injector,
    });
  }
  close(): void {
    this.dialogRef.close();
  }
}
