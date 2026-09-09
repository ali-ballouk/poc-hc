export type SelectorMode = 'single' | 'multi';

export interface SelectorConfig<T> {
  valueField: keyof T | string;
  textField: keyof T | string;
  mode: SelectorMode;
}

export interface SelectedItem<T> {
  value: any;
  text: string;
  item: T;
}
