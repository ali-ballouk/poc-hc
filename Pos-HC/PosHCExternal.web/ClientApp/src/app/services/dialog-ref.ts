import { InjectionToken } from '@angular/core';
import { ReplaySubject } from 'rxjs';

export const DIALOG_DATA = new InjectionToken<any>('DIALOG_DATA');

export class DialogRef<R = any> {
  private readonly closed = new ReplaySubject<R | undefined>(1);
  constructor(private readonly requestClose: (result?: R) => void) {}
  close(result?: R): void {
    this.requestClose(result);
  }
  afterClosed() {
    return this.closed.asObservable();
  }
  complete(result?: R): void {
    this.closed.next(result);
    this.closed.complete();
  }
}
