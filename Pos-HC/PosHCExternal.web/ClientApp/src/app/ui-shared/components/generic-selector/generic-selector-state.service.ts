import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Service to manage global state of open selector dropdowns.
 * Ensures only one selector dropdown is open at a time across the page.
 */
@Injectable({ providedIn: 'root' })
export class SelectorStateService {
  private openSelectorId$ = new Subject<string | null>();
  private openDropdownId: string | null = null;

  /**
   * Notifies all selectors about which dropdown should be open
   */
  getOpenSelectorObservable() {
    return this.openSelectorId$.asObservable();
  }

  /**
   * Registers a selector as the currently open dropdown
   */
  setOpenSelector(id: string): void {
    if (this.openDropdownId !== id) {
      this.openDropdownId = id;
      this.openSelectorId$.next(id);
    }
  }

  /**
   * Closes the currently open selector (if any)
   */
  closeOpenSelector(id?: string): void {
    if (id && id !== this.openDropdownId) return;
    if (this.openDropdownId !== null) {
      this.openDropdownId = null;
      this.openSelectorId$.next(null);
    }
  }

  /**
   * Checks if a specific selector is the currently open one
   */
  isOpen(id: string): boolean {
    return this.openDropdownId === id;
  }
}
