import { TestBed } from '@angular/core/testing';
import { GenericGridComponent } from './generic-grid.component';

describe('GenericGridComponent', () => {
  function createGrid() {
    const fixture = TestBed.createComponent(GenericGridComponent<any>);
    fixture.componentRef.setInput('data', [
      { id: 1, name: 'Alice', amount: 30, date: new Date('2025-01-01') },
      { id: 2, name: 'Bob', amount: 5, date: new Date('2024-01-01') },
      { id: 3, name: 'Carol', amount: 20, date: new Date('2026-01-01') },
    ]);
    fixture.componentRef.setInput('rowId', (row: any) => row.id);
    fixture.componentRef.setInput('columns', [
      { key: 'name', header: 'Name' },
      { key: 'amount', header: 'Amount', type: 'currency' },
      { key: 'date', header: 'Date', type: 'date' },
    ]);
    fixture.componentRef.setInput('pageSize', 2);
    fixture.detectChanges();
    return fixture;
  }

  it('sorts amounts and dates, filters rows and resets pagination', () => {
    const fixture = createGrid(),
      grid = fixture.componentInstance;
    grid.nextPage();
    expect(grid.pagedRows.map((r) => r.id)).toEqual([3]);
    grid.toggleSort(grid.columns[1]);
    grid.previousPage();
    expect(grid.pagedRows.map((r) => r.id)).toEqual([2, 3]);
    grid.toggleSort(grid.columns[2]);
    expect(grid.pagedRows.map((r) => r.id)).toEqual([2, 1]);
    const search = fixture.nativeElement.querySelector('input[type="search"]');
    search.value = 'Carol';
    search.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(grid.pageIndex).toBe(0);
    expect(grid.pagedRows.map((r) => r.id)).toEqual([3]);
    grid.onPageSizeChanged(0);
    expect(grid.pageSize).toBe(10);
  });

  it('keeps column menus and control IDs independent for multiple grids', () => {
    const first = createGrid(),
      second = createGrid();
    const buttons = Array.from(
      first.nativeElement.querySelectorAll('button'),
    ) as HTMLButtonElement[];
    buttons.find((button) => button.textContent?.trim() === 'Columns')!.click();
    first.detectChanges();
    expect(first.componentInstance.columnMenuOpen).toBeTrue();
    expect(second.componentInstance.columnMenuOpen).toBeFalse();
    expect(first.componentInstance.gridId).not.toBe(
      second.componentInstance.gridId,
    );
    first.componentInstance.setColumnHidden(
      first.componentInstance.columns[0],
      true,
    );
    expect(first.componentInstance.visibleColumns.length).toBe(2);
    expect(first.componentInstance.columns[0].hidden).toBeUndefined();
    second.destroy();
  });

  it('runs the action for the displayed row and prunes selection on data replacement', () => {
    const fixture = createGrid(),
      grid = fixture.componentInstance;
    const handler = jasmine.createSpy('handler');
    fixture.componentRef.setInput('actions', [{ label: 'Remove', handler }]);
    fixture.detectChanges();
    const row = grid.pagedRows[0];
    grid.toggleRow(row, true);
    fixture.nativeElement.querySelector('.data-row button').click();
    expect(handler).toHaveBeenCalledWith(row);
    const changed = jasmine.createSpy('selection');
    grid.selectionChanged.subscribe(changed);
    fixture.componentRef.setInput(
      'data',
      grid.data.filter((r) => r.id !== row.id),
    );
    fixture.detectChanges();
    expect(grid.selectedRows).toEqual([]);
    expect(changed).toHaveBeenCalledWith([]);
  });
});
