import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BidiModule } from '@angular/cdk/bidi';
import { LanguageService } from './services/language';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, BidiModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  languageService = inject(LanguageService);

  constructor()
  {
    this.languageService.init();
  }
}