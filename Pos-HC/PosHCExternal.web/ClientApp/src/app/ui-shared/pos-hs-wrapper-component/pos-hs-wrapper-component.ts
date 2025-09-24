import {
  Component,
  Input,
  ViewChild,
  ViewContainerRef,
  ComponentRef,
  OnInit,
  OnChanges,
  SimpleChanges,
  Type
} from '@angular/core';
@Component({
  selector: 'app-dynamic-wrapper',
  template: `<ng-container #vc></ng-container>`
})
export class PosHsWrapperComponent implements OnInit {
  @Input() component!: Type<any> | null ;      
  @Input() inputs: any = null;       

  @ViewChild('vc', { read: ViewContainerRef, static: true })
  vc!: ViewContainerRef;

  private cmpRef!: ComponentRef<any>;

  ngOnInit() {
    this.loadComponent();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['component'] && this.component) {
      this.loadComponent();
    } else if (this.cmpRef && changes['inputs']) {
      // update inputs dynamically
      Object.keys(this.inputs || {}).forEach(key => {
        this.cmpRef.instance[key] = this.inputs[key];
      });
    }
  }

  private loadComponent() {
    this.vc.clear();
    if (!this.component) return;

    this.cmpRef = this.vc.createComponent(this.component);

    Object.keys(this.inputs || {}).forEach(key => {
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
