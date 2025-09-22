import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BaseAPI } from '../../services/base.api';

import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';


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
export class VisitItemsComponent implements OnInit {
  // sample items for the select
 
  items = signal<any[]>([]);


  ngOnInit(): void {
    this.api.get<any[]>('api/item').subscribe({
      next: (res) => {
        this.items.set(res);
      },
      error: (err) => console.error('Error loading items', err)
    });
  }
  constructor(private api: BaseAPI) { }

  selectedItemId: string | null = null;

  displayedColumns: string[] = ['name', 'unitPrice', 'type' , 'description'];
  dataSource = new MatTableDataSource<any>([]);


  settingsToString(settings: Record<string, any>): string {
    if (!settings) return '';
    return Object.values(settings).join(' / ');
  }


  getVisitItems(): string[] {
    return this.dataSource.data.map((x: any) => x.id)
  }
  addItem() {
    if (!this.selectedItemId) return;

    const selected = this.items().find(d => d.Id === this.selectedItemId);
    if (!selected) return;

    // clone the item with quantity
    const row = {
      id: selected.Id,
      name: selected.Name,
      unitPrice: selected.UnitPrice,
      type: selected.Type == 1 ? 'Product' : 'Service',
      description: this.settingsToString(selected.Settings)
    };

    this.dataSource.data = [...this.dataSource.data, row];

    // reset select
    this.selectedItemId = null
  }
}
