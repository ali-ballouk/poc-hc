import { TranslatePipe } from './i18n/language';
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
import { LanguageService } from './i18n/language';

@Component({
  selector: 'app-root',
  imports: [TranslatePipe, LayoutComponent, LoginComponent],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.css',
})
export class App {
  language = inject(LanguageService);
  auth = inject(AuthService);
  constructor() {
    this.auth.initialize();
  }
}
