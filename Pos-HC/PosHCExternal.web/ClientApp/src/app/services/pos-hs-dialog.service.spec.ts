import { Component, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DialogService } from './pos-hs-dialog.service';
import { DIALOG_DATA, DialogRef } from './dialog-ref';

@Component({
  template:
    '<p>{{ data.message }}</p><button class="test-close" (click)="ref.close({success: true})">Done</button>',
})
class TestContent {
  data = inject(DIALOG_DATA);
  ref = inject(DialogRef);
}

describe('DialogService', () => {
  it('returns from a nested form to its invoice without closing the invoice', () => {
    const service = TestBed.inject(DialogService);
    const parent = service.openComponent(TestContent, 'Invoice', {
      message: 'Parent invoice',
    });
    const parentElement = document.querySelector<HTMLElement>('.modal')!;
    const trigger =
      parentElement.querySelector<HTMLButtonElement>('.test-close')!;
    trigger.focus();
    let parentClosed = false;
    parent.afterClosed().subscribe(() => (parentClosed = true));
    const child = service.openComponent(
      TestContent,
      'Payment',
      { message: 'Payment form' },
      { nested: true },
    );
    expect(document.querySelectorAll('.modal.show').length).toBe(1);
    expect(parentElement.style.display).toBe('none');
    expect(parentClosed).toBeFalse();
    child.close({ saved: true });
    expect(document.querySelectorAll('.modal').length).toBe(1);
    expect(parentElement.classList.contains('show')).toBeTrue();
    expect(document.activeElement).toBe(trigger);
    expect(document.body.classList.contains('modal-open')).toBeTrue();
    parent.close();
    expect(parentClosed).toBeTrue();
    expect(document.querySelector('.modal-backdrop')).toBeNull();
    expect(document.body.classList.contains('modal-open')).toBeFalse();
  });
  it('renders dynamic content, returns its result, cleans up and restores focus', () => {
    const service = TestBed.inject(DialogService);
    const trigger = document.createElement('button');
    document.body.appendChild(trigger);
    trigger.focus();
    const ref = service.openComponent(TestContent, 'Payment test', {
      message: 'Invoice details',
    });
    let result: any;
    ref.afterClosed().subscribe((value) => (result = value));
    const modal = document.querySelector<HTMLElement>('.modal')!;
    expect(modal.textContent).toContain('Invoice details');
    expect(modal.getAttribute('aria-modal')).toBe('true');
    expect(document.body.classList.contains('modal-open')).toBeTrue();
    modal.querySelector<HTMLButtonElement>('.test-close')!.click();
    expect(result).toEqual({ success: true });
    expect(document.querySelector('.modal')).toBeNull();
    expect(document.querySelector('.modal-backdrop')).toBeNull();
    expect(document.body.classList.contains('modal-open')).toBeFalse();
    expect(document.activeElement).toBe(trigger);
    trigger.remove();
  });

  it('dismisses from the close button without a success result', () => {
    const ref = TestBed.inject(DialogService).openComponent(
      TestContent,
      'Cancel',
      { message: '' },
    );
    let emitted = false;
    ref.afterClosed().subscribe((value) => {
      emitted = true;
      expect(value).toBeUndefined();
    });
    document.querySelector<HTMLButtonElement>('.modal .btn-close')!.click();
    expect(emitted).toBeTrue();
    expect(document.querySelector('.modal')).toBeNull();
  });
});
