import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { LayoutComponent } from './pos-hc-sidebar';

@Component({ template: '' })
class EmptyPage {}

describe('LayoutComponent', () => {
  let component: LayoutComponent;
  let fixture: ComponentFixture<LayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LayoutComponent],
      providers: [
        provideRouter([{ path: 'billing', component: EmptyPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  it('selects Invoices on the billing route, including patient filters', async () => {
    component.auth.user.set({ Username: 'admin', Role: 'Administrator' });
    fixture.detectChanges();
    await TestBed.inject(Router).navigateByUrl('/billing?patientId=patient-1');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('a[href="/billing"]');
    expect(link.classList.contains('active')).toBeTrue();
    expect(link.getAttribute('aria-current')).toBe('page');
  });
  it('toggles the desktop sidebar without changing the mobile menu state', () => {
    const button = fixture.nativeElement.querySelector(
      '.sidebar-toggle.d-none',
    );
    button.click();
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.sidebar-collapsed'),
    ).not.toBeNull();
    expect(button.getAttribute('aria-expanded')).toBe('false');
    expect(component.navigationOpen).toBeFalse();
    button.click();
    fixture.detectChanges();
    expect(button.getAttribute('aria-expanded')).toBe('true');
  });
});
