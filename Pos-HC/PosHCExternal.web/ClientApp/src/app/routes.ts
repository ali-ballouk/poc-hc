import { Routes } from '@angular/router';
import { PosHcEditor } from './pos-hc-editor/pos-hc-editor';
import { PosHcInvoices } from './invoices/pos-hc-invoices'; 

export const routes: Routes = [
    { path: 'pointofsale', component: PosHcEditor },
    { path: 'invoices', component: PosHcInvoices },
    { path: '**', redirectTo: '' } // fallback
];
