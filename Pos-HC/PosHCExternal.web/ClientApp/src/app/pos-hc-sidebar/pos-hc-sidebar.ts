import { Component, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    RouterModule
  ],
  template: `
  <mat-sidenav-container class="h-screen">
    <mat-sidenav #sidenav mode="side" opened class="w-60 bg-gray-100">
      <mat-toolbar color="primary">POS HC</mat-toolbar>
      <mat-nav-list>
        <a mat-list-item routerLink="/pointofsale" routerLinkActive="bg-blue-200">
          <mat-icon>medical_services</mat-icon>
          <span class="ml-2">Point Of Sale</span>
        </a>
        <a mat-list-item routerLink="/invoices" routerLinkActive="bg-blue-200">
          <mat-icon>receipt</mat-icon>
          <span class="ml-2">Invoice</span>
        </a>
      </mat-nav-list>
    </mat-sidenav>

    <mat-sidenav-content>
      <mat-toolbar color="primary">
        <button mat-icon-button (click)="sidenav.toggle()" class="md:hidden">
          <mat-icon>menu</mat-icon>
        </button>
        <span>POS HC Dashboard</span>
      </mat-toolbar>

      <div class="p-4">
        <router-outlet></router-outlet>
      </div>
    </mat-sidenav-content>
  </mat-sidenav-container>
  `
})
export class LayoutComponent {
  @ViewChild('sidenav') sidenav!: MatSidenav;
}
