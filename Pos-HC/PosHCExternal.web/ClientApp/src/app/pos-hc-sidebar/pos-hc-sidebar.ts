import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { AuthService } from '../management/auth.service';
import { modules } from '../management/module-definitions';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-sidebar.html',
})
export class LayoutComponent {
  auth = inject(AuthService);
  modules = Object.entries(modules).map(([key, definition]) => ({
    key,
    ...definition,
  }));
  navigationOpen = false;
}
