import { TestBed } from '@angular/core/testing';
import { LanguageService, LocalNumberPipe, TranslatePipe } from './language';

describe('Language support', () => {
  afterEach(() => TestBed.inject(LanguageService).setLanguage('en'));
  it('switches the document direction and persists the selected language', () => {
    const service = TestBed.inject(LanguageService);
    service.setLanguage('ar');
    expect(document.documentElement.lang).toBe('ar');
    expect(document.documentElement.dir).toBe('rtl');
    expect(localStorage.getItem('pos-hc-language')).toBe('ar');
    service.setLanguage('en');
    expect(document.documentElement.dir).toBe('ltr');
  });
  it('refreshes existing pipes and preserves unknown record text', () => {
    const service = TestBed.inject(LanguageService);
    const pipe = TestBed.runInInjectionContext(() => new TranslatePipe());
    service.setLanguage('en');
    expect(pipe.transform('Invoices')).toBe('Invoices');
    service.setLanguage('ar');
    expect(pipe.transform('Invoices')).toBe('الفواتير');
    expect(pipe.transform('مريم Haddad')).toBe('مريم Haddad');
    expect(pipe.transform('Edit Patients')).toBe('تعديل المرضى');
  });
  it('formats amounts with the selected locale without changing precision', () => {
    const service = TestBed.inject(LanguageService);
    const pipe = TestBed.runInInjectionContext(() => new LocalNumberPipe());
    service.setLanguage('ar');
    expect(pipe.transform(1234.5, '1.2-2')).toBe(
      new Intl.NumberFormat('ar-LB', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(1234.5),
    );
  });
});
