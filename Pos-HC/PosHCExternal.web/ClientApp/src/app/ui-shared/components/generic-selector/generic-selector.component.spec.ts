import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GenericSelectorComponent } from './generic-selector.component';
import { SelectorStateService } from './generic-selector-state.service';

interface Option {
  id: number;
  name: string;
}
describe('GenericSelectorComponent', () => {
  let fixture: ComponentFixture<GenericSelectorComponent<Option>>;
  const options = [
    { id: 1, name: 'Alice' },
    { id: 2, name: 'Bob' },
  ];
  beforeEach(() => {
    fixture = TestBed.createComponent(GenericSelectorComponent<Option>);
    fixture.componentRef.setInput('valueField', 'id');
    fixture.componentRef.setInput('textField', 'name');
    fixture.componentRef.setInput('mode', 'single');
    fixture.componentRef.setInput('data', []);
    fixture.componentRef.setInput('placeholder', 'Choose person');
  });

  it('positions a fixed menu below the button and follows captured container scroll', () => {
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const dropdown = fixture.nativeElement.querySelector(
      '.dropdown',
    ) as HTMLElement;
    const button = fixture.nativeElement.querySelector(
      '.dropdown-toggle',
    ) as HTMLElement;
    dropdown.dir = 'ltr';
    let top = 200;
    spyOn(dropdown, 'getBoundingClientRect').and.callFake(
      () => new DOMRect(120, top, 240, 40),
    );
    spyOn(button, 'getBoundingClientRect').and.callFake(
      () => new DOMRect(120, top, 240, 40),
    );
    component.openDropdown();
    fixture.detectChanges();
    const menu = fixture.nativeElement.querySelector(
      '.dropdown-menu',
    ) as HTMLElement;
    expect(getComputedStyle(menu).position).toBe('fixed');
    expect(menu.style.top).toBe('242px');
    expect(menu.style.left).toBe('120px');
    expect(menu.style.right).toBe('auto');
    expect(menu.style.width).toBe('240px');
    top = 80;
    dropdown.dispatchEvent(new Event('scroll'));
    fixture.detectChanges();
    expect(menu.style.top).toBe('122px');
    component.closeDropdown();
    top = 10;
    window.dispatchEvent(new Event('resize'));
    expect(component.menuTop).toBe(122);
  });

  it('switches an open dropdown to right alignment when direction changes', async () => {
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const dropdown = fixture.nativeElement.querySelector(
      '.dropdown',
    ) as HTMLElement;
    const button = fixture.nativeElement.querySelector(
      '.dropdown-toggle',
    ) as HTMLElement;
    dropdown.dir = 'ltr';
    spyOn(dropdown, 'getBoundingClientRect').and.returnValue(
      new DOMRect(100, 50, 200, 45),
    );
    spyOn(button, 'getBoundingClientRect').and.returnValue(
      new DOMRect(100, 50, 200, 45),
    );
    component.openDropdown();
    fixture.detectChanges();
    dropdown.dir = 'rtl';
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
    const menu = fixture.nativeElement.querySelector(
      '.dropdown-menu',
    ) as HTMLElement;
    expect(menu.style.left).toBe('auto');
    expect(menu.style.right).toBe(
      `${document.documentElement.clientWidth - 300}px`,
    );
    expect(menu.style.top).toBe('97px');
    const remove = spyOn(window, 'removeEventListener').and.callThrough();
    fixture.destroy();
    expect(remove).toHaveBeenCalledWith(
      'scroll',
      component.updateDropdownPosition,
      true,
    );
    expect(remove).toHaveBeenCalledWith(
      'resize',
      component.updateDropdownPosition,
    );
  });

  it('synchronizes asynchronous data and external resets without emitting user changes', () => {
    const changed = jasmine.createSpy('changed');
    fixture.componentInstance.selectionChanged.subscribe(changed);
    fixture.componentRef.setInput('initialSelection', 2);
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.selector-text').textContent,
    ).toBe('Choose person');
    fixture.componentRef.setInput('data', options);
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.selector-text').textContent,
    ).toBe('Bob');
    fixture.componentRef.setInput('initialSelection', null);
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.selector-text').textContent,
    ).toBe('Choose person');
    expect(changed).not.toHaveBeenCalled();
  });

  it('retains selections outside the search filter and clears the complete set', () => {
    fixture.componentRef.setInput('mode', 'multi');
    fixture.componentRef.setInput('data', options);
    fixture.componentRef.setInput('initialSelection', [1]);
    fixture.detectChanges();
    const changed = jasmine.createSpy('changed');
    fixture.componentInstance.selectionChanged.subscribe(changed);
    fixture.componentInstance.onSearchChanged('Bob');
    fixture.componentInstance.toggleSelectAll();
    expect(changed).toHaveBeenCalledWith(options);
    fixture.componentInstance.toggleSelectAll();
    expect(changed).toHaveBeenCalledWith([options[0]]);
    fixture.componentInstance.clearSearch();
    expect(
      fixture.componentInstance.selectedOptions.map((o) => o.item),
    ).toEqual([options[0]]);
    fixture.componentInstance.clearAll();
    expect(changed).toHaveBeenCalledWith([]);
  });

  it('keeps ownership with the newly opened selector', () => {
    fixture.detectChanges();
    const other = TestBed.createComponent(GenericSelectorComponent<Option>);
    for (const [key, value] of Object.entries({
      valueField: 'id',
      textField: 'name',
      mode: 'single',
      data: options,
    }))
      other.componentRef.setInput(key, value);
    other.detectChanges();
    fixture.componentInstance.openDropdown();
    other.componentInstance.openDropdown();
    expect(fixture.componentInstance.isOpen).toBeFalse();
    expect(other.componentInstance.isOpen).toBeTrue();
    expect(
      TestBed.inject(SelectorStateService).isOpen(
        other.componentInstance.inputId,
      ),
    ).toBeTrue();
    fixture.destroy();
    expect(other.componentInstance.isOpen).toBeTrue();
    other.destroy();
  });

  it('supports keyboard selection and Escape focus restoration', async () => {
    fixture.componentRef.setInput('data', options);
    fixture.detectChanges();
    const trigger = fixture.nativeElement.querySelector(
      '.dropdown-toggle',
    ) as HTMLButtonElement;
    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();
    const search = fixture.nativeElement.querySelector(
      'input',
    ) as HTMLInputElement;
    search.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true }),
    );
    const option = fixture.nativeElement.querySelector(
      '[role="option"]',
    ) as HTMLButtonElement;
    expect(document.activeElement).toBe(option);
    option.click();
    fixture.detectChanges();
    expect(fixture.componentInstance.displayText).toBe('Alice');
    expect(document.activeElement).toBe(trigger);
    trigger.click();
    fixture.detectChanges();
    trigger.dispatchEvent(
      new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }),
    );
    expect(fixture.componentInstance.isOpen).toBeFalse();
    expect(document.activeElement).toBe(trigger);
  });
});
