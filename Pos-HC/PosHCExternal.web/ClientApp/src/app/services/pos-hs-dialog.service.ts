import {
  ApplicationRef,
  EnvironmentInjector,
  Injectable,
  Injector,
  Type,
  createComponent,
  inject,
} from '@angular/core';
import { DOCUMENT } from '@angular/common';
import Modal from 'bootstrap/js/dist/modal';
import { PosHcDialogShell } from '../ui-shared/pos-hc-dialog-shell/pos-hc-dialog-shell';
import { DIALOG_DATA, DialogRef } from './dialog-ref';

@Injectable({ providedIn: 'root' })
export class DialogService {
  private readonly app = inject(ApplicationRef);
  private readonly environmentInjector = inject(EnvironmentInjector);
  private readonly injector = inject(Injector);
  private readonly document = inject(DOCUMENT);
  private active?: DialogRef;

  openComponent<T>(component: Type<T>, title?: string, data?: any): DialogRef {
    if (this.active) return this.active;
    const previousFocus = this.document.activeElement as HTMLElement | null;
    const modalElement = this.document.createElement('div');
    modalElement.className = 'modal';
    modalElement.tabIndex = -1;
    modalElement.setAttribute('aria-labelledby', 'pos-dialog-title');
    modalElement.innerHTML =
      '<div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable"><div class="modal-content"></div></div>';
    const host = this.document.createElement('app-dialog-shell');
    modalElement.querySelector('.modal-content')!.appendChild(host);
    this.document.body.appendChild(modalElement);

    let result: any;
    const modal = new Modal(modalElement, {
      backdrop: 'static',
      keyboard: false,
      focus: true,
    });
    const ref = new DialogRef((value) => {
      result = value;
      modal.hide();
    });
    const shell = createComponent(PosHcDialogShell, {
      hostElement: host,
      environmentInjector: this.environmentInjector,
      elementInjector: Injector.create({
        parent: this.injector,
        providers: [
          { provide: DIALOG_DATA, useValue: { component, title, data } },
          { provide: DialogRef, useValue: ref },
        ],
      }),
    });
    this.app.attachView(shell.hostView);
    shell.changeDetectorRef.detectChanges();
    modalElement.addEventListener(
      'hidden.bs.modal',
      () => {
        this.app.detachView(shell.hostView);
        shell.destroy();
        modal.dispose();
        modalElement.remove();
        this.active = undefined;
        previousFocus?.focus();
        ref.complete(result);
      },
      { once: true },
    );
    this.active = ref;
    modal.show();
    return ref;
  }
}
