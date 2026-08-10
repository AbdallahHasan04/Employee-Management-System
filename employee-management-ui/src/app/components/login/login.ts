import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth';
import { LanguageSwitcherComponent } from '../language-switcher/language-switcher';

const LOCKOUT_STORAGE_KEY = 'login_lockout_until';

@Component
(
  {
  selector: 'app-login',
  imports: 
  [
  CommonModule, 
  ReactiveFormsModule,
  MatFormFieldModule, 
  MatInputModule, 
  MatButtonModule,
  MatIconModule,
  MatProgressSpinnerModule,
  TranslatePipe,
  LanguageSwitcherComponent
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
  }
)

export class LoginComponent implements OnInit, OnDestroy
{
  private formbuilder = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private translate = inject(TranslateService);
  private cdr = inject(ChangeDetectorRef);

  loginform : FormGroup;
  errormessage = '';
  isSubmitting = false;
  hidePassword = true;

  isLockedOut = false;
  lockoutRemainingSeconds = 0;
  private lockoutIntervalId: ReturnType<typeof setInterval> | undefined;

  constructor()
  {
    this.loginform = this.formbuilder.group
    (
      {
      username: ['', Validators.required],
      password: ['', Validators.required]
      }
    );
  }

  ngOnInit(): void
  {
    const storedUntil = localStorage.getItem(LOCKOUT_STORAGE_KEY);
    if (storedUntil) {
      const remainingMs = Number(storedUntil) - Date.now();
      if (remainingMs > 0) {
        this.beginCountdown(Math.ceil(remainingMs / 1000));
      } else {
        localStorage.removeItem(LOCKOUT_STORAGE_KEY);
      }
    }
  }

  get lockoutTimeDisplay(): string
  {
    const minutes = Math.floor(this.lockoutRemainingSeconds / 60);
    const seconds = this.lockoutRemainingSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  togglePasswordVisibility(): void
  {
    this.hidePassword = !this.hidePassword;
  }

  onSubmit()
  {
    if (this.isLockedOut) return;

    if(this.loginform.valid){
      const { username, password } = this.loginform.value;

      this.isSubmitting = true;
      this.authService.login({ username, password }).pipe(
        finalize(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
        })
      ).subscribe({
        next: (response) => {
          this.errormessage = '';
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error('Login error details:', err);
          if (err.status === 429) {
            const seconds = err.error?.lockoutRemainingSeconds ?? 900;
            const until = Date.now() + seconds * 1000;
            localStorage.setItem(LOCKOUT_STORAGE_KEY, until.toString());
            this.beginCountdown(seconds);
          } else {
            this.errormessage = this.translate.instant('login.invalidCredentials');
          }
          this.cdr.detectChanges();
        }
      });
    }
  }

  private beginCountdown(seconds: number): void
  {
    this.isLockedOut = true;
    this.lockoutRemainingSeconds = seconds;
    this.errormessage = '';
    this.loginform.disable();
    this.cdr.detectChanges();

    clearInterval(this.lockoutIntervalId);
    this.lockoutIntervalId = setInterval(() => {
      this.lockoutRemainingSeconds--;
      if (this.lockoutRemainingSeconds <= 0) {
        this.endLockout();
      }
      this.cdr.detectChanges();
    }, 1000);
  }

  private endLockout(): void
  {
    this.isLockedOut = false;
    this.loginform.enable();
    clearInterval(this.lockoutIntervalId);
    localStorage.removeItem(LOCKOUT_STORAGE_KEY);
  }

  ngOnDestroy(): void
  {
    clearInterval(this.lockoutIntervalId);
  }
}