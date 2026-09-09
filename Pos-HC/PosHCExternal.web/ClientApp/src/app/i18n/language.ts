import { DOCUMENT } from '@angular/common';
import {
  Injectable,
  Pipe,
  PipeTransform,
  computed,
  inject,
  signal,
} from '@angular/core';
import { arabic } from './translations';

export type Language = 'en' | 'ar';
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private document = inject(DOCUMENT);
  private current = signal<Language>('en');
  readonly language = this.current.asReadonly();
  readonly locale = computed(() =>
    this.language() === 'ar' ? 'ar-LB' : 'en-US',
  );
  constructor() {
    let saved: string | null = null;
    try {
      saved =
        this.document.defaultView?.localStorage.getItem('pos-hc-language') ??
        null;
    } catch {
      /* Storage may be disabled. */
    }
    this.setLanguage(saved === 'ar' ? 'ar' : 'en');
  }
  setLanguage(language: Language) {
    this.current.set(language);
    this.document.documentElement.lang = language;
    this.document.documentElement.dir = language === 'ar' ? 'rtl' : 'ltr';
    try {
      this.document.defaultView?.localStorage.setItem(
        'pos-hc-language',
        language,
      );
    } catch {
      /* The current session still works. */
    }
  }
  translate(value: unknown): string {
    if (value == null) return '';
    const text = String(value);
    if (this.language() === 'en') return text;
    const key = text.replace(/\s+/g, ' ').trim();
    if (arabic[key.toLowerCase()]) return arabic[key.toLowerCase()];
    for (const prefix of ['Edit ', 'Add ', 'Search ', 'Clear ', 'Resize ']) {
      if (key.startsWith(prefix))
        return (
          this.translate(prefix.trim()) +
          ' ' +
          this.translate(key.slice(prefix.length))
        );
    }
    if (key.endsWith(' *')) return this.translate(key.slice(0, -2)) + ' *';
    if (key.endsWith(' is required.'))
      return this.translate(key.slice(0, -13)) + ' مطلوب.';
    return text;
  }
}

@Pipe({ name: 'localNumber', standalone: true, pure: false })
export class LocalNumberPipe implements PipeTransform {
  private language = inject(LanguageService);
  transform(
    value: number | string | null | undefined,
    digits = '1.0-2',
  ): string {
    if (value == null || value === '') return '';
    const [integer, fraction = '0-3'] = digits.split('.');
    const [min, max = min] = fraction.split('-');
    return new Intl.NumberFormat(this.language.locale(), {
      minimumIntegerDigits: +integer,
      minimumFractionDigits: +min,
      maximumFractionDigits: +max,
    }).format(Number(value));
  }
}

@Pipe({ name: 't', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private language = inject(LanguageService);
  transform(value: unknown): string {
    return this.language.translate(value);
  }
}
