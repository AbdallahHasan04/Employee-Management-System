import { Component, Input, inject } from '@angular/core';
import { LanguageService } from '../../services/language';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-language-switcher',
  imports: [TranslatePipe],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.css',
})
export class LanguageSwitcherComponent
{
  @Input() variant: 'dark' | 'light' = 'dark';

  languageService = inject(LanguageService);

  toggle(): void
  {
    this.languageService.toggleLanguage();
  }
}