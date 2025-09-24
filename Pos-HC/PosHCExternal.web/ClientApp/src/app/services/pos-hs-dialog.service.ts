import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { PosHcDialogShell, DialogShellData } from '../ui-shared/pos-hc-dialog-shell/pos-hc-dialog-shell';
import { ComponentType } from '@angular/cdk/portal';

@Injectable({ providedIn: 'root' })
export class DialogService {
  constructor(private dialog: MatDialog) { }

  openComponent<T>(
    component: ComponentType<T>,
    title?: string,
    data?: any
  ): MatDialogRef<PosHcDialogShell> {
    return this.dialog.open(PosHcDialogShell, {
      width: '700px',
      disableClose: true,
      data: <DialogShellData>{
        title,
        component,
        data
      }
    });
  }
}
