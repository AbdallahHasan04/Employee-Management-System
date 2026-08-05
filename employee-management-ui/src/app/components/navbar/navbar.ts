import { Component, inject, ViewContainerRef } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../services/auth';
import { LanguageService } from '../../services/language';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher';
import { ChangePasswordDialogComponent } from '../change-password-dialog/change-password-dialog';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class NavbarComponent
{
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private viewContainerRef = inject(ViewContainerRef);
  private languageService = inject(LanguageService);

  onChangePassword(): void
  {
    this.dialog.open(ChangePasswordDialogComponent, {
      viewContainerRef: this.viewContainerRef,
      direction: this.languageService.dir()
    });
  }

  onLogout(): void
  {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}