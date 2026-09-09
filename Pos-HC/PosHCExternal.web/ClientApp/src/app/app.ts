import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';

import { LayoutComponent } from './pos-hc-sidebar/pos-hc-sidebar';
import { AuthService } from './management/auth.service';
import { LoginComponent } from './management/login.component';

@Component({
  selector: 'app-root',
  imports: [LayoutComponent, LoginComponent],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.css',
})
export class App {
  auth = inject(AuthService);
  constructor() {
    this.auth.initialize();
  }
}
