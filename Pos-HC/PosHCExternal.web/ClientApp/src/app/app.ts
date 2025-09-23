import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';


import { LayoutComponent } from './pos-hc-sidebar/pos-hc-sidebar';


@Component({
  selector: 'app-root',
  imports: [LayoutComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App  {
  
}
