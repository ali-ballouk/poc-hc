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
