import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslatePipe } from '@ngx-translate/core';

export interface ConfirmDialogData {
  title: string;
  message: string;
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [MatDialogModule, MatButtonModule, TranslatePipe],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialogComponent
{
  data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);

  onCancel(): void
  {
    this.dialogRef.close(false);
  }

  onConfirm(): void
  {
    this.dialogRef.close(true);
  }
}