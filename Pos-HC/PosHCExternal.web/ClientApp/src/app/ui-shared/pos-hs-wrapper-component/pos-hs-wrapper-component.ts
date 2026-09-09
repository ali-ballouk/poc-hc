import {
  Component,
  Input,
  ViewChild,
  Type,
  ChangeDetectionStrategy,
} from '@angular/core';
import { NgComponentOutlet } from '@angular/common';

@Component({
  selector: 'app-dynamic-wrapper',
  imports: [NgComponentOutlet],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './pos-hs-wrapper-component.html',
})
export class PosHsWrapperComponent {
  @Input() component: Type<any> | null = null;
  @Input() inputs: Record<string, unknown> | null = null;
  @ViewChild(NgComponentOutlet) outlet?: NgComponentOutlet;

  getData() {
    return this.outlet?.componentInstance?.getData?.() ?? null;
  }
}
