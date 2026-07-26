import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class SnackbarService
{
  private snackBar = inject(MatSnackBar);

  showSuccess(message: string): void
  {
    this.snackBar.open(message, undefined, {
      duration: 3000,
      panelClass: ['snackbar-success'],
      horizontalPosition: 'end',
      verticalPosition: 'top',
    });
  }

  showError(message: string): void
  {
    this.snackBar.open(message, undefined, {
      duration: 4000,
      panelClass: ['snackbar-error'],
      horizontalPosition: 'end',
      verticalPosition: 'top',
    });
  }
}