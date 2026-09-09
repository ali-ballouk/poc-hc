import { Routes } from '@angular/router';
import { PosHcEditor } from './pos-hc-editor/pos-hc-editor';
import { PosHcInvoices } from './invoices/pos-hc-invoices';
import { ManagementComponent } from './management/management.component';
import { BillingComponent } from './management/billing.component';
import { SettingsComponent } from './management/settings.component';
import { ReportsComponent } from './management/reports.component';
import { roleGuard } from './management/role.guard';

export const routes: Routes = [
  {
    path: 'pointofsale',
    component: PosHcEditor,
    canActivate: [roleGuard],
    data: { roles: ['Administrator', 'Receptionist', 'Cashier'] },
  },
  { path: 'invoices', redirectTo: 'billing', pathMatch: 'full' },
  {
    path: 'billing',
    component: BillingComponent,
    canActivate: [roleGuard],
    data: { roles: ['Administrator', 'Receptionist', 'Cashier'] },
  },
  {
    path: 'manage/:module',
    component: ManagementComponent,
    canActivate: [roleGuard],
  },
  {
    path: 'settings',
    component: SettingsComponent,
    canActivate: [roleGuard],
    data: { roles: ['Administrator'] },
  },
  {
    path: 'reports',
    component: ReportsComponent,
    canActivate: [roleGuard],
    data: { roles: ['Administrator'] },
  },
  { path: '', pathMatch: 'full', redirectTo: 'pointofsale' },
  { path: '**', redirectTo: 'pointofsale' }, // fallback
];
