import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const STORAGE_KEY = 'app_lang';
const RTL_LANGS = ['ar'];

@Injectable({ providedIn: 'root' })
export class LanguageService
{
  private translate = inject(TranslateService);

  dir = signal<'ltr' | 'rtl'>('ltr');

  init(): void
  {
    const saved = localStorage.getItem(STORAGE_KEY);
    const lang = saved === 'ar' || saved === 'en' ? saved : 'en';
    this.setLanguage(lang);
  }

  getCurrentLang(): string
  {
    return this.translate.getCurrentLang() ?? 'en';
  }

  toggleLanguage(): void
  {
    const next = this.getCurrentLang() === 'en' ? 'ar' : 'en';
    this.setLanguage(next);
  }

  setLanguage(lang: string): void
  {
    this.translate.use(lang);
    localStorage.setItem(STORAGE_KEY, lang);

    const direction = RTL_LANGS.includes(lang) ? 'rtl' : 'ltr';
    document.documentElement.lang = lang;
    document.documentElement.dir = direction;
    this.dir.set(direction);
  }
}