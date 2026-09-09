import { InjectionToken, TemplateRef } from '@angular/core';

export type SortDirection = 'asc' | 'desc';
export type GridColumnType =
  | 'text'
  | 'number'
  | 'date'
  | 'boolean'
  | 'currency';

export const GENERIC_GRID_HOST = new InjectionToken<unknown>(
  'GENERIC_GRID_HOST',
);

export interface GridColumn<T> {
  key: keyof T | string;
  header: string;
  type?: GridColumnType;
  sortable?: boolean;
  filterable?: boolean;
  hidden?: boolean;
  widthPx?: number;
  minWidthPx?: number;
  cellTemplate?: TemplateRef<{ $implicit: T; value: unknown }>;
  valueAccessor?: (row: T) => unknown;
}

export interface GridAction<T> {
  icon?: string;
  label: string;
  color?: 'primary' | 'accent' | 'warn';
  handler: (row: T) => void;
}

export interface GridSort<T> {
  columnKey: keyof T | string;
  direction: SortDirection;
}
