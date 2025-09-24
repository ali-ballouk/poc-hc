import { Component, OnInit, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BaseAPI } from '../services/base.api';

import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';
import { MatIcon, MatIconModule } from '@angular/material/icon';

interface Invoice {
  InvoiceId: string;
  InvoiceDate: Date;
  DoctorId: string;
  DoctorName:string;
  PatientId: string;
  PatientName:string
  Discount:Number
  DoctorFee: Number;
  Total: Number;
  Items: any[]
}
@Component({
  selector: 'pos-hs-invoices',
  imports:  [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatInputModule,
    MatTableModule,
    MatIconModule,
    MatIcon
  ],
  templateUrl: './pos-hc-invoices.html'
})

export class PosHcInvoices implements OnInit {

  constructor(private api: BaseAPI) { }


  dataSource = new MatTableDataSource<any>([]);

  ngOnInit(): void {
    this.api.get<any[]>('api/invoice').subscribe({
      next: (res) => {
        this.bindaData(res);
      },
      error: (err) => console.error('Error loading invoices', err)
    });
  }

  bindaData(response: Invoice[]) {
    this.dataSource.data = response.map(x => ({
      ...x,
      InvoiceDate: new Date(x.InvoiceDate) // string → Date
    }));
  }

  displayedColumns: string[] = [
    'InvoiceId',
    'InvoiceDate',
    'DoctorName',
    'PatientName',
    'DoctorFee', 
    'Discount',
    'Total',
    'actions'
  ];

  downloadInvoice(id: string) {
    let thisApiUrl = `api/invoice/${id}/print`;
    this.api.downloadPdf<any[]>(thisApiUrl).subscribe({
      next: (res) => {
          const dispo = res.headers.get('Content-Disposition');
          const match = /filename="?([^"]+)"?/.exec(dispo || '');
          const fileName = match ? match[1] : `Invoice-${id}.pdf`;

          const blob = new Blob([res.body!], { type: 'application/pdf' });
          const url = URL.createObjectURL(blob);

          const a = document.createElement('a');
          a.href = url;
          a.download = fileName;
          a.click();

          URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Error loading invoices', err)
    });

    this.dataSource.data = this.dataSource.data.filter(x => x.id !== id);
  }


  

}
