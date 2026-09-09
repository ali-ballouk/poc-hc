import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { VisitItemsComponent } from './visit-item-component';

describe('VisitItemsComponent', () => {
  it('adds duplicate catalog items as distinct grid rows and removes only the chosen row', async () => {
    await TestBed.configureTestingModule({
      imports: [VisitItemsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(VisitItemsComponent);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/catalogtitem').flush([
      {
        Id: 'consultation',
        Name: 'Consultation',
        UnitPrice: 25,
        Type: 2,
        Settings: {},
      },
    ]);
    fixture.detectChanges();
    const totals: number[] = [];
    fixture.componentInstance.itemsChanged.subscribe((value) =>
      totals.push(value),
    );
    function add() {
      fixture.nativeElement.querySelector('.dropdown-toggle').click();
      fixture.detectChanges();
      fixture.nativeElement.querySelector('[role="option"]').click();
      fixture.detectChanges();
      fixture.nativeElement.querySelector('button.w-100').click();
      fixture.detectChanges();
    }
    const quantity = fixture.nativeElement.querySelector('#item-quantity');
    quantity.value = '2';
    quantity.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    add();
    add();
    expect(fixture.componentInstance.getVisitItems()).toEqual([
      { CatalogItemId: 'consultation', Quantity: 2 },
      { CatalogItemId: 'consultation', Quantity: 2 },
    ]);
    expect(fixture.nativeElement.querySelectorAll('.data-row').length).toBe(2);
    expect(
      fixture.nativeElement.querySelector('.selector-text').textContent,
    ).toContain('Choose item');
    fixture.nativeElement.querySelector('.data-row button').click();
    fixture.detectChanges();
    expect(fixture.componentInstance.rows.length).toBe(1);
    expect(totals).toEqual([50, 100, 50]);
    fixture.nativeElement.querySelector('.data-row button').click();
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.empty-state').textContent,
    ).toContain('No items added yet.');
    http.verify();
  });
});
