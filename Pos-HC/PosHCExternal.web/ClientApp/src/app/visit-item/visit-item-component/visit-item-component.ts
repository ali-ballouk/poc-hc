import { TranslatePipe } from '../../i18n/language';
import {
  Component,
  OnInit,
  signal,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { GenericGridComponent } from '../../ui-shared/components/generic-grid/generic-grid.component';
import {
  GridAction,
  GridColumn,
} from '../../ui-shared/components/generic-grid/generic-grid.models';
import { GenericSelectorComponent } from '../../ui-shared/components/generic-selector/generic-selector.component';
import { BaseAPI } from '../../services/base.api';

import { FormsModule } from '@angular/forms';

interface VisitRow {
  rowId: number;
  id: string;
  name: string;
  unitPrice: number;
  type: string;
  quantity: number;
  description: string;
}

@Component({
  selector: 'pos-hs-visit-item',
  standalone: true,
  imports: [
    TranslatePipe,
    FormsModule,
    GenericGridComponent,
    GenericSelectorComponent,
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './visit-item.component.html',
})
export class VisitItemsComponent implements OnInit {
  // sample items for the select
  @Output() itemsChanged = new EventEmitter<number>(); // 👈 Parent will listen

  items = signal<any[]>([]);

  ngOnInit(): void {
    this.api.get<any[]>('api/catalogitem/lookup').subscribe({
      next: (res) => {
        this.items.set(res);
      },
      error: (err) => console.error('Error loading items', err),
    });
  }
  constructor(private api: BaseAPI) {}

  selectedItemId: string | null = null;

  quantity: number = 1;

  rows: VisitRow[] = [];
  private nextRowId = 0;
  readonly rowId = (row: VisitRow) => row.rowId;
  readonly columns: GridColumn<VisitRow>[] = [
    { key: 'name', header: 'Item name', widthPx: 190 },
    { key: 'type', header: 'Type', widthPx: 110 },
    { key: 'description', header: 'Description', widthPx: 190 },
    { key: 'unitPrice', header: 'Unit price', type: 'currency', widthPx: 130 },
    { key: 'quantity', header: 'Quantity', type: 'number', widthPx: 110 },
    {
      key: 'total',
      header: 'Total',
      type: 'currency',
      widthPx: 130,
      valueAccessor: (row) => row.unitPrice * row.quantity,
    },
  ];
  readonly actions: GridAction<VisitRow>[] = [
    {
      label: 'Remove',
      color: 'warn',
      handler: (row) => this.removeItem(row.rowId),
    },
  ];

  settingsToString(settings: Record<string, any>): string {
    if (!settings) return '';
    return Object.values(settings).join(' / ');
  }

  getVisitItems(): any[] {
    return this.rows.map((x: any) => ({
      CatalogItemId: x.id,
      Quantity: x.quantity,
    }));
  }

  clearItems() {
    if (this.selectedItemId) this.selectedItemId = null;
    this.rows = [];
    this.quantity = 1;
    this.emitTotal();
  }
  addItem() {
    if (!this.selectedItemId) return;

    const selected = this.items().find((d) => d.Id === this.selectedItemId);
    if (!selected) return;
    const quantity = this.quantity;
    if (!Number.isInteger(quantity) || quantity < 1 || quantity > 10000) return;
    // clone the item with quantity
    const row: VisitRow = {
      rowId: this.nextRowId++,
      id: selected.Id,
      name: selected.Name,
      unitPrice: selected.UnitPrice,
      type: selected.Type == 1 ? 'Product' : 'Service',
      quantity: quantity,
      description: this.settingsToString(selected.Settings),
    };

    this.rows = [...this.rows, row];
    this.selectedItemId = null;
    this.emitTotal();
  }

  removeItem(rowId: number) {
    this.rows = this.rows.filter((x) => x.rowId !== rowId);
    this.emitTotal();
  }

  private emitTotal() {
    const total = this.rows.reduce(
      (sum, x) => sum + x.unitPrice * x.quantity,
      0,
    );
    this.itemsChanged.emit(total);
  }
}
