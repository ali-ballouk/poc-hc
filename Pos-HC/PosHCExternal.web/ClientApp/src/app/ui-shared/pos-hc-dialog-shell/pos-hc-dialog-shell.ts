// dialog-shell.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { PortalModule, ComponentPortal } from '@angular/cdk/portal';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface DialogShellData {
  title?: string;
  component: any;   
  data?: any; 
}

@Component({
  selector: 'app-dialog-shell',
  standalone: true,
  imports: [CommonModule, MatDialogModule, PortalModule, MatIconModule, MatButtonModule],
  template: `
    <div mat-dialog-title class="flex items-center justify-between" style="display:flex">
      <span>{{data.title || 'Dialog'}}</span>
      <button mat-icon-button (click)="close()" class="flex justify-end" aria-label="Close">
        <mat-icon>close</mat-icon>
      </button>
    </div>

    <div mat-dialog-content>
      <ng-template [cdkPortalOutlet]="portal"></ng-template>
    </div>
  `
})
export class PosHcDialogShell {
  portal: ComponentPortal<any>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: DialogShellData,
    private dialogRef: MatDialogRef<PosHcDialogShell>
  ) {
    this.portal = new ComponentPortal(data.component);
  }

  close() {
    this.dialogRef.close();
  }
}
