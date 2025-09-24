import {
  Component,
  Input,
  ViewChild,
  ViewContainerRef,
  ComponentRef,
  OnInit,
  Type
} from '@angular/core';

@Component({
  selector: 'app-dynamic-wrapper',
  template: `<ng-container #vc></ng-container>`
})
export class PosHsWrapperComponent implements OnInit {
  @Input() component!: Type<any>;      
  @Input() inputs: any = {};       

  @ViewChild('vc', { read: ViewContainerRef, static: true })
  vc!: ViewContainerRef;

  private cmpRef!: ComponentRef<any>;

  ngOnInit() {
    this.vc.clear();
    this.cmpRef = this.vc.createComponent(this.component);

    // مرر الـ inputs
    Object.keys(this.inputs).forEach(key => {
      this.cmpRef.instance[key] = this.inputs[key];
    });
  }

  getData() {
    if (this.cmpRef?.instance?.getData) {
      return this.cmpRef.instance.getData();
    }
    return null;
  }
}
