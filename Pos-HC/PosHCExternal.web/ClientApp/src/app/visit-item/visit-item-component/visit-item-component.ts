import { Component, OnInit, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BaseAPI } from '../../services/base.api';

import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';
import { MatIcon, MatIconModule } from '@angular/material/icon';



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
    MatInputModule,
    MatTableModule,
    MatIconModule,
    MatIcon
  ],
  templateUrl: './visit-item.component.html'
})
export class VisitItemsComponent implements OnInit {
  // sample items for the select
  @Output() itemsChanged = new EventEmitter<number>();  // 👈 Parent will listen

  items = signal<any[]>([]);


  ngOnInit(): void {
    this.api.get<any[]>('api/catalogtitems').subscribe({
      next: (res) => {
        this.items.set(res);
      },
      error: (err) => console.error('Error loading items', err)
    });
  }
  constructor(private api: BaseAPI) { }

  selectedItemId: string | null = null;

  quantity: number = 1;

  displayedColumns: string[] = ['name', 'type', 'description', 'unitPrice','quantity' ,'total','actions'];
  dataSource = new MatTableDataSource<any>([]);


  settingsToString(settings: Record<string, any>): string {
    if (!settings) return '';
    return Object.values(settings).join(' / ');
  }


  getVisitItems(): any[] {
    return this.dataSource.data.map((x: any) => ({ CatalogItemId: x.id, Quantity: x.quantity}))
  }

  clearItems() {
    if (this.selectedItemId) this.selectedItemId = null;
    this.dataSource.data = [];
    this.emitTotal();
  }
  addItem() {
    if (!this.selectedItemId) return;

    const selected = this.items().find(d => d.Id === this.selectedItemId);
    if (!selected) return;
    const quantity = this.quantity;
    // clone the item with quantity
    const row = {
      id: selected.Id,
      name: selected.Name,
      unitPrice: selected.UnitPrice,
      type: selected.Type == 1 ? 'Product' : 'Service',
      quantity: quantity,
      description: this.settingsToString(selected.Settings)
    };

    this.dataSource.data = [...this.dataSource.data, row];
    this.selectedItemId = null
    this.emitTotal();

  }

  removeItem(id: string) {
    this.dataSource.data = this.dataSource.data.filter(x => x.id !== id);
    this.emitTotal();
  }

  private emitTotal() {
    const total = this.dataSource.data.reduce((sum, x) => sum + x.unitPrice * x.quantity, 0);
    this.itemsChanged.emit(total);
  }
}
