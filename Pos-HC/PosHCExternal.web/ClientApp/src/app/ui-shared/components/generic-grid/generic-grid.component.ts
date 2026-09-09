import { TranslatePipe, LanguageService } from '../../../i18n/language';
import { CommonModule } from '@angular/common';
import {
  AfterContentInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ContentChildren,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  QueryList,
  SimpleChanges,
  forwardRef,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { GenericGridColumnComponent } from './generic-grid-column.component';
import {
  GENERIC_GRID_HOST,
  GridAction,
  GridColumn,
  GridSort,
  SortDirection,
} from './generic-grid.models';

let nextGridId = 0;

@Component({
  selector: 'app-generic-grid',
  standalone: true,
  imports: [TranslatePipe, CommonModule, FormsModule],
  templateUrl: './generic-grid.component.html',
  styleUrl: './generic-grid.component.css',
  providers: [
    {
      provide: GENERIC_GRID_HOST,
      useExisting: forwardRef(() => GenericGridComponent),
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericGridComponent<T extends object = any>
  implements OnChanges, AfterContentInit, OnDestroy
{
  @Input({ required: true }) data: readonly T[] = [];
  @Input() columns: readonly GridColumn<T>[] = [];
  @Input() actions: readonly GridAction<T>[] = [];
  @Input() loading = false;
  @Input() label = 'Data grid';
  language = inject(LanguageService);
  @Input() emptyMessage = 'No data available';
  readonly gridId = 'grid-' + nextGridId++;
  columnMenuOpen = false;
  private readonly hiddenColumns = new Map<string, boolean>();
  private resizing?: { key: string; pointerId: number; x: number };
  @Input() pageSizeOptions: readonly number[] = [5, 10, 20];
  @Input() pageSize = 10;
  @Input() defaultSort?: GridSort<T>;
  @Input() selectable = true;
  @Input() rowId: (row: T) => string | number = (row) =>
    String(JSON.stringify(row));

  @Output() rowClicked = new EventEmitter<T>();
  @Output() selectionChanged = new EventEmitter<readonly T[]>();

  @ContentChildren(GenericGridColumnComponent)
  projectedColumns?: QueryList<GenericGridColumnComponent<T>>;

  private readonly changeDetector = inject(ChangeDetectorRef);
  private projectedColumnsChanges?: Subscription;

  searchTerm = '';
  pageIndex = 0;
  sort?: GridSort<T>;
  selectedIds = new Set<string | number>();
  resizedWidths = new Map<string, number>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['defaultSort'])
      this.sort = this.defaultSort ? { ...this.defaultSort } : undefined;
    if (changes['pageSize']) this.onPageSizeChanged(this.pageSize);

    if (changes['data']) {
      this.pageIndex = 0;
      this.pruneSelection();
    }
  }

  ngAfterContentInit(): void {
    this.projectedColumnsChanges = this.projectedColumns?.changes.subscribe(
      () => {
        this.pageIndex = 0;
        this.changeDetector.markForCheck();
      },
    );
  }

  ngOnDestroy(): void {
    this.projectedColumnsChanges?.unsubscribe();
  }

  get configuredColumns(): readonly GridColumn<T>[] {
    const projected = this.projectedColumns?.toArray() ?? [];
    return projected.length > 0 ? projected : this.columns;
  }

  get visibleColumns(): readonly GridColumn<T>[] {
    return this.configuredColumns.filter(
      (column) => !this.isColumnHidden(column),
    );
  }

  get filteredRows(): readonly T[] {
    const term = this.searchTerm.trim().toLocaleLowerCase();
    const filterableColumns = this.configuredColumns.filter(
      (column) => column.filterable !== false,
    );

    if (!term) {
      return [...this.data];
    }

    return this.data.filter((row) =>
      filterableColumns.some((column) =>
        String(this.getCellValue(row, column) ?? '')
          .toLocaleLowerCase()
          .includes(term),
      ),
    );
  }

  get sortedRows(): readonly T[] {
    const rows = [...this.filteredRows];

    if (!this.sort) {
      return rows;
    }

    const column = this.configuredColumns.find(
      (candidate) => candidate.key === this.sort?.columnKey,
    );
    if (!column) {
      return rows;
    }

    return rows.sort((left, right) => {
      const leftValue = this.getCellValue(left, column);
      const rightValue = this.getCellValue(right, column);
      const comparison = this.compareValues(leftValue, rightValue);
      return this.sort?.direction === 'asc' ? comparison : -comparison;
    });
  }

  get pagedRows(): readonly T[] {
    const start = this.pageIndex * this.pageSize;
    return this.sortedRows.slice(start, start + this.pageSize);
  }

  get pageCount(): number {
    return Math.max(1, Math.ceil(this.sortedRows.length / this.pageSize));
  }

  get pageStart(): number {
    return this.sortedRows.length === 0
      ? 0
      : this.pageIndex * this.pageSize + 1;
  }

  get pageEnd(): number {
    return Math.min(
      this.sortedRows.length,
      (this.pageIndex + 1) * this.pageSize,
    );
  }

  get selectedRows(): readonly T[] {
    return this.data.filter((row) => this.selectedIds.has(this.rowId(row)));
  }

  get allPageRowsSelected(): boolean {
    return (
      this.pagedRows.length > 0 &&
      this.pagedRows.every((row) => this.selectedIds.has(this.rowId(row)))
    );
  }

  get partiallySelected(): boolean {
    return (
      this.pagedRows.some((row) => this.selectedIds.has(this.rowId(row))) &&
      !this.allPageRowsSelected
    );
  }

  onSearchChanged(): void {
    this.pageIndex = 0;
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.pageIndex = 0;
  }

  toggleSort(column: GridColumn<T>): void {
    if (column.sortable === false) {
      return;
    }

    const isSameColumn = this.sort?.columnKey === column.key;
    const nextDirection: SortDirection =
      isSameColumn && this.sort?.direction === 'asc' ? 'desc' : 'asc';
    this.sort = { columnKey: column.key, direction: nextDirection };
  }

  setColumnHidden(column: GridColumn<T>, hidden: boolean): void {
    this.hiddenColumns.set(String(column.key), hidden);
  }

  isColumnHidden(column: GridColumn<T>): boolean {
    return this.hiddenColumns.get(String(column.key)) ?? column.hidden ?? false;
  }

  onPageSizeChanged(value: string | number): void {
    const size = Number(value);
    this.pageSize =
      Number.isFinite(size) && size > 0 ? Math.max(1, Math.floor(size)) : 10;
    this.pageIndex = 0;
  }

  previousPage(): void {
    this.pageIndex = Math.max(0, this.pageIndex - 1);
  }

  nextPage(): void {
    this.pageIndex = Math.min(this.pageCount - 1, this.pageIndex + 1);
  }

  toggleRow(row: T, selected: boolean): void {
    const id = this.rowId(row);
    selected ? this.selectedIds.add(id) : this.selectedIds.delete(id);
    this.selectionChanged.emit(this.selectedRows);
  }

  togglePage(selected: boolean): void {
    for (const row of this.pagedRows) {
      const id = this.rowId(row);
      selected ? this.selectedIds.add(id) : this.selectedIds.delete(id);
    }

    this.selectionChanged.emit(this.selectedRows);
  }

  onRowClicked(row: T): void {
    this.rowClicked.emit(row);
  }

  getCellValue(row: T, column: GridColumn<T>): unknown {
    if (column.valueAccessor) {
      return column.valueAccessor(row);
    }

    return (row as Record<string, unknown>)[String(column.key)];
  }

  formatCellValue(row: T, column: GridColumn<T>): string {
    const value = this.getCellValue(row, column);

    if (value == null || value === '') {
      return '';
    }

    switch (column.type ?? 'text') {
      case 'number':
        return typeof value === 'number'
          ? new Intl.NumberFormat(this.language.locale()).format(value)
          : String(value);
      case 'date': {
        const date = value instanceof Date ? value : new Date(String(value));
        return Number.isNaN(date.getTime())
          ? String(value)
          : new Intl.DateTimeFormat(this.language.locale()).format(date);
      }
      case 'boolean':
        return this.language.translate(value ? 'Yes' : 'No');
      case 'currency':
        return typeof value === 'number'
          ? new Intl.NumberFormat(this.language.locale(), {
              style: 'currency',
              currency: 'USD',
            }).format(value)
          : String(value);
      default:
        if (typeof value === 'number')
          return new Intl.NumberFormat(this.language.locale()).format(value);
        if (typeof value === 'boolean')
          return this.language.translate(value ? 'Yes' : 'No');
        if (
          [
            'Status',
            'PaymentStatus',
            'Kind',
            'Role',
            'Method',
            'Type',
            'type',
          ].includes(String(column.key))
        )
          return this.language.translate(value);
        return String(value);
    }
  }

  getColumnWidth(column: GridColumn<T>): number {
    return this.resizedWidths.get(String(column.key)) ?? column.widthPx ?? 160;
  }

  resizeColumn(column: GridColumn<T>, movementX: number): void {
    const key = String(column.key);
    const minWidth = column.minWidthPx ?? 88;
    const nextWidth = Math.max(
      minWidth,
      this.getColumnWidth(column) + movementX,
    );
    this.resizedWidths.set(key, nextWidth);
  }

  startResize(column: GridColumn<T>, event: PointerEvent): void {
    if (event.button !== 0) return;
    event.preventDefault();
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    this.resizing = {
      key: String(column.key),
      pointerId: event.pointerId,
      x: event.clientX,
    };
  }

  moveResize(column: GridColumn<T>, event: PointerEvent): void {
    if (
      this.resizing?.key !== String(column.key) ||
      this.resizing?.pointerId !== event.pointerId
    )
      return;
    this.resizeColumn(
      column,
      (event.clientX - this.resizing.x) *
        (this.language.language() === 'ar' ? -1 : 1),
    );
    this.resizing.x = event.clientX;
  }

  endResize(): void {
    this.resizing = undefined;
  }

  trackByRow = (_: number, row: T): string | number => this.rowId(row);
  trackByColumn = (_: number, column: GridColumn<T>): string =>
    String(column.key);

  private compareValues(left: unknown, right: unknown): number {
    if (left == null && right == null) {
      return 0;
    }

    if (left == null) {
      return -1;
    }

    if (right == null) {
      return 1;
    }

    if (left instanceof Date && right instanceof Date)
      return left.getTime() - right.getTime();

    if (typeof left === 'number' && typeof right === 'number') {
      return left - right;
    }

    return String(left).localeCompare(String(right), undefined, {
      numeric: true,
      sensitivity: 'base',
    });
  }

  private pruneSelection(): void {
    const currentIds = new Set(this.data.map((row) => this.rowId(row)));
    let changed = false;

    for (const selectedId of this.selectedIds) {
      if (!currentIds.has(selectedId)) {
        this.selectedIds.delete(selectedId);
        changed = true;
      }
    }

    if (changed) {
      this.selectionChanged.emit(this.selectedRows);
    }
  }
}
