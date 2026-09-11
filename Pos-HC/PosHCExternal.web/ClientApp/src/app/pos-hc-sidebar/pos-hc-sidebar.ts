import { TranslatePipe } from '../i18n/language';
import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { AuthService } from '../management/auth.service';
import { modules } from '../management/module-definitions';
import { RouterModule } from '@angular/router';
import {
  mdiAccountHeart,
  mdiAccountKey,
  mdiCalendarClock,
  mdiCalendarMonth,
  mdiCashRegister,
  mdiChartBar,
  mdiClipboardTextClock,
  mdiClose,
  mdiCog,
  mdiDoctor,
  mdiMedicalBag,
  mdiMenu,
  mdiMenuClose,
  mdiMenuOpen,
  mdiReceiptText,
  mdiSafe,
} from '@mdi/js';

const moduleIcons: Record<string, string> = {
  patients: mdiAccountHeart,
  doctors: mdiDoctor,
  catalog: mdiMedicalBag,
  staff: mdiAccountKey,
  appointments: mdiCalendarMonth,
  availability: mdiCalendarClock,
  shifts: mdiSafe,
  audit: mdiClipboardTextClock,
};

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [TranslatePipe, RouterModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hc-sidebar.html',
})
export class LayoutComponent {
  readonly icons = {
    pointOfSale: mdiCashRegister,
    invoices: mdiReceiptText,
    reports: mdiChartBar,
    settings: mdiCog,
    menu: mdiMenu,
    close: mdiClose,
    expand: mdiMenuClose,
    collapse: mdiMenuOpen,
  };
  auth = inject(AuthService);
  modules = Object.entries(modules).map(([key, definition]) => ({
    key,
    ...definition,
    icon: moduleIcons[key] ?? mdiMedicalBag,
  }));
  navigationOpen = false;
  sidebarCollapsed = false;
}
