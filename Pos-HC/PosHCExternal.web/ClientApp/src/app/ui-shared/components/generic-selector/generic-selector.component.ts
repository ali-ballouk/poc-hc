import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges,
  ViewChild,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { SelectedItem, SelectorMode } from './generic-selector.models';
import { SelectorStateService } from './generic-selector-state.service';

let nextSelectorId = 0;

@Component({
  selector: 'app-generic-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './generic-selector.component.html',
  styleUrl: './generic-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericSelectorComponent<T extends object = any>
  implements OnChanges, OnInit, OnDestroy
{
  @Input({ required: true }) data: readonly T[] = [];
  @Input({ required: true }) valueField!: keyof T | string;
  @Input({ required: true }) textField!: keyof T | string;
  @Input({ required: true }) mode: SelectorMode = 'single';
  @Input() placeholder = 'Select an option...';
  @Input() label = '';
  @Input() disabled = false;
  // Bound IDs stay synchronized when the parent clears or changes a selection.
  @Input() initialSelection: unknown = null;
  @Input() inputId = `selector-${nextSelectorId++}`;

  @Output() selectionChanged = new EventEmitter<T | T[] | null>();
  @ViewChild('trigger') trigger?: ElementRef<HTMLButtonElement>;
  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;
  @ViewChild('dropdownMenu') dropdownMenu?: ElementRef<HTMLElement>;

  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly elementRef = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly selectorState = inject(SelectorStateService);
  private readonly destroy$ = new Subject<void>();

  isOpen = false;
  selectedItems = new Set<string>();
  displayText = '';
  allOptions: SelectedItem<T>[] = [];
  filteredOptions: SelectedItem<T>[] = [];
  searchTerm = '';
  allItemsSelected = false;

  get selectedOptions(): SelectedItem<T>[] {
    return this.allOptions.filter((option) => this.isOptionSelected(option));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes['initialSelection'] ||
      changes['mode'] ||
      changes['valueField']
    ) {
      this.initializeSelection();
    }
    this.buildOptionsList();
    if (this.disabled) this.closeDropdown();
  }

  ngOnInit(): void {
    this.initializeSelection();
    this.buildOptionsList();
    this.selectorState
      .getOpenSelectorObservable()
      .pipe(takeUntil(this.destroy$))
      .subscribe((openId) => {
        if (openId !== this.inputId) this.closeDropdown();
      });
  }

  ngOnDestroy(): void {
    this.selectorState.closeOpenSelector(this.inputId);
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target as Node))
      this.closeDropdown();
  }

  @HostListener('focusout', ['$event'])
  onFocusOut(event: FocusEvent): void {
    if (
      event.relatedTarget &&
      !this.elementRef.nativeElement.contains(event.relatedTarget as Node)
    )
      this.closeDropdown();
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (this.disabled) return;
    if (event.key === 'Escape' && this.isOpen) {
      event.preventDefault();
      event.stopPropagation();
      this.closeDropdown(true);
      return;
    }
    if (!['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) return;
    if (!this.isOpen) {
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        this.openDropdown();
      }
      return;
    }
    const options = Array.from(
      this.elementRef.nativeElement.querySelectorAll<HTMLElement>(
        '[role="option"], .options-list input',
      ),
    );
    if (!options.length) return;
    // Home/End retain their editing behavior in the search input.
    if (
      event.target === this.searchInput?.nativeElement &&
      ['Home', 'End'].includes(event.key)
    )
      return;
    event.preventDefault();
    const current = options.indexOf(event.target as HTMLElement);
    const index =
      event.key === 'Home'
        ? 0
        : event.key === 'End'
          ? options.length - 1
          : event.key === 'ArrowDown'
            ? (current + 1) % options.length
            : (current <= 0 ? options.length : current) - 1;
    options[index].focus();
  }

  private initializeSelection(): void {
    const values =
      this.mode === 'multi'
        ? Array.isArray(this.initialSelection)
          ? this.initialSelection
          : []
        : this.initialSelection == null || this.initialSelection === ''
          ? []
          : [this.initialSelection];
    this.selectedItems = new Set(values.map(String));
  }

  private buildOptionsList(): void {
    this.allOptions = this.data.map((item) => ({
      value: this.getPropertyValue(item, this.valueField),
      text: String(this.getPropertyValue(item, this.textField) ?? ''),
      item,
    }));
    const term = this.searchTerm.trim().toLocaleLowerCase();
    this.filteredOptions = this.allOptions.filter((option) =>
      option.text.toLocaleLowerCase().includes(term),
    );
    this.updateSelectAllState();
    this.updateDisplayText();
  }

  private getPropertyValue(item: T, key: keyof T | string): unknown {
    return (item as Record<string, unknown>)[String(key)];
  }

  private updateDisplayText(): void {
    this.displayText =
      this.mode === 'single'
        ? (this.selectedOptions[0]?.text ?? this.placeholder)
        : this.selectedItems.size
          ? `${this.selectedItems.size} selected`
          : this.placeholder;
  }

  openDropdown(): void {
    if (this.disabled || this.isOpen) return;
    this.isOpen = true;
    this.buildOptionsList();
    this.selectorState.setOpenSelector(this.inputId);
    this.changeDetector.markForCheck();
    queueMicrotask(() => {
      if (this.isOpen) this.searchInput?.nativeElement.focus();
    });
  }

  closeDropdown(restoreFocus = false): void {
    this.isOpen = false;
    this.searchTerm = '';
    this.buildOptionsList();
    this.selectorState.closeOpenSelector(this.inputId);
    this.changeDetector.markForCheck();
    if (restoreFocus) this.trigger?.nativeElement.focus();
  }

  toggleDropdown(): void {
    this.isOpen ? this.closeDropdown() : this.openDropdown();
  }

  onOptionClicked(option: SelectedItem<T>): void {
    if (this.disabled || this.mode !== 'single') return;
    const selected = this.isOptionSelected(option);
    this.selectedItems.clear();
    if (!selected) this.selectedItems.add(String(option.value));
    this.updateDisplayText();
    this.selectionChanged.emit(selected ? null : option.item);
    this.closeDropdown(true);
  }

  toggleOptionSelection(option: SelectedItem<T>, event: Event): void {
    event.stopPropagation();
    if (this.disabled) return;
    const id = String(option.value);
    this.selectedItems.has(id)
      ? this.selectedItems.delete(id)
      : this.selectedItems.add(id);
    this.updateSelectAllState();
    this.emitMultiSelection();
  }

  isOptionSelected(option: SelectedItem<T>): boolean {
    return this.selectedItems.has(String(option.value));
  }

  onSearchChanged(term: string): void {
    this.searchTerm = term;
    this.buildOptionsList();
  }

  clearSearch(): void {
    this.onSearchChanged('');
  }

  clearSelection(): void {
    if (this.disabled) return;
    this.selectedItems.clear();
    this.updateSelectAllState();
    this.updateDisplayText();
    this.selectionChanged.emit(this.mode === 'multi' ? [] : null);
    this.changeDetector.markForCheck();
  }

  private emitMultiSelection(): void {
    this.updateDisplayText();
    this.selectionChanged.emit(
      this.selectedOptions.map((option) => option.item),
    );
    this.changeDetector.markForCheck();
  }

  getSelectedCount(): number {
    return this.selectedItems.size;
  }
  clearAll(): void {
    this.clearSelection();
  }

  toggleSelectAll(): void {
    if (this.disabled) return;
    for (const option of this.filteredOptions) {
      this.allItemsSelected
        ? this.selectedItems.delete(String(option.value))
        : this.selectedItems.add(String(option.value));
    }
    this.updateSelectAllState();
    this.emitMultiSelection();
  }

  updateSelectAllState(): void {
    this.allItemsSelected =
      this.filteredOptions.length > 0 &&
      this.filteredOptions.every((option) => this.isOptionSelected(option));
  }

  trackByOption = (_: number, option: SelectedItem<T>): string =>
    String(option.value);
}
