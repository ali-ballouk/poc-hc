import { Component } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';

interface VisitItem {
  id: number;
  name: string;
  unitPrice: number;
}

@Component({
  selector: 'pos-hs-visit-item',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatTableModule
  ],
  templateUrl: './visit-item.component.html'
})
export class VisitItemsComponent {
  // sample items for the select
  items: VisitItem[] = [
    { id: 1, name: 'Blood Test', unitPrice: 50 },
    { id: 2, name: 'Consultation', unitPrice: 100 },
    { id: 3, name: 'X-Ray', unitPrice: 150 }
  ];

  selectedItemId: number | null = null;

  displayedColumns: string[] = ['name', 'unitPrice', 'quantity'];
  dataSource = new MatTableDataSource<any>([]);

  quantity: number = 1;

  addItem() {
    if (!this.selectedItemId) return;

    const selected = this.items.find(i => i.id === this.selectedItemId);
    if (!selected) return;

    // clone the item with quantity
    const row = {
      name: selected.name,
      unitPrice: selected.unitPrice,
      quantity: this.quantity
    };

    this.dataSource.data = [...this.dataSource.data, row];

    // reset select
    this.selectedItemId = null;
    this.quantity = 1;
  }
}
