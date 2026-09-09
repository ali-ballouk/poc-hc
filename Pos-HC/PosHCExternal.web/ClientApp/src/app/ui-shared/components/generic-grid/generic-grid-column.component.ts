import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ContentChild,
  Inject,
  Input,
  Optional,
  TemplateRef,
} from '@angular/core';
import {
  GENERIC_GRID_HOST,
  GridColumn,
  GridColumnType,
} from './generic-grid.models';

@Component({
  selector: 'app-generic-grid-column',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './generic-grid-column.component.html',
  styles: [':host { display: none; }'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericGridColumnComponent<T extends object = any>
  implements GridColumn<T>
{
  @Input({ required: true }) key!: keyof T | string;
  @Input({ required: true }) header!: string;
  @Input() type: GridColumnType = 'text';
  @Input() sortable = true;
  @Input() filterable = true;
  @Input() hidden = false;
  @Input() widthPx?: number;
  @Input() minWidthPx?: number;
  @Input() valueAccessor?: (row: T) => unknown;

  @ContentChild(TemplateRef)
  cellTemplate?: TemplateRef<{ $implicit: T; value: unknown }>;

  constructor(@Optional() @Inject(GENERIC_GRID_HOST) gridHost: unknown) {
    if (!gridHost) {
      throw new Error(
        'app-generic-grid-column must be used inside app-generic-grid.',
      );
    }
  }
}
