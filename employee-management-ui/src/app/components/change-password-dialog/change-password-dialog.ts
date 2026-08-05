import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../services/auth';
import { SnackbarService } from '../../services/snackbar';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null
{
    const newPassword = control.parent?.get('newPassword')?.value;
    const confirmNewPassword = control.value;

    if (!newPassword || !confirmNewPassword) {
        return null; // let required handle empty fields
    }

    return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-change-password-dialog',
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, TranslatePipe
  ],
  templateUrl: './change-password-dialog.html',
  styleUrl: './change-password-dialog.css',
})
export class ChangePasswordDialogComponent
{
  private formBuilder = inject(FormBuilder);
  private authService = inject(AuthService);
  private translate = inject(TranslateService);
  private snackbar = inject(SnackbarService);
  private dialogRef = inject(MatDialogRef<ChangePasswordDialogComponent>);

  form: FormGroup;
  isSubmitting = false;
  errorMessage = '';

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  constructor()
  {
    this.form = this.formBuilder.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required, passwordsMatchValidator]]
    });

    // Re-check the match whenever newPassword changess
    this.form.get('newPassword')!.valueChanges.subscribe(() => {
      this.form.get('confirmNewPassword')!.updateValueAndValidity({ onlySelf: true });
    });
  }

  onSubmit(): void
  {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const { currentPassword, newPassword } = this.form.value;

    this.authService.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.snackbar.showSuccess(this.translate.instant('changePassword.success'));
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          this.errorMessage = error.error?.message || this.translate.instant('changePassword.incorrectCurrentPassword');
        } else {
          this.errorMessage = this.translate.instant('changePassword.error');
        }
      }
    });
  }

  onCancel(): void
  {
    this.dialogRef.close(false);
  }
}