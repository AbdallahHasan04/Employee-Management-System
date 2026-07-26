import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { DepartmentService, Department, NewDepartment } from '../../services/department';
import { NavbarComponent } from '../navbar/navbar';
import { ConfirmDialogComponent, ConfirmDialogData } from '../confirm-dialog/confirm-dialog';
import { SnackbarService } from '../../services/snackbar';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-departments',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatIconModule, MatProgressSpinnerModule, NavbarComponent, TranslatePipe,
  ],
  templateUrl: './departments.html',
  styleUrl: './departments.css',
})
export class DepartmentsComponent implements OnInit
{
  private departmentService = inject(DepartmentService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private dialog = inject(MatDialog);
  private snackbar = inject(SnackbarService);

  departments: Department[] = [];
  displayedColumns: string[] = ['departmentCode', 'nameEn', 'nameAr', 'description', 'status', 'actions'];

  newDepartment: NewDepartment = this.emptyNewDepartment();
  editingDepartment: Department | null = null;

  isSubmitting = false;
  deletingId: number | null = null;
  togglingId: number | null = null;

  ngOnInit()
  {
    this.loadData();
  }

  private emptyNewDepartment(): NewDepartment
  {
    return { departmentCode: '', nameEn: '', nameAr: '', description: null };
  }

  loadData()
  {
    this.departmentService.getDepartments().subscribe({
      next: (data) => {
        this.departments = [...data];
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        this.snackbar.showError(this.translate.instant('departments.fetchError'));
      }
    });
  }

  onAdd(): void
  {
    const required = [this.newDepartment.departmentCode, this.newDepartment.nameEn, this.newDepartment.nameAr];
    if (required.some(f => !f?.trim())) {
      this.snackbar.showError(this.translate.instant('departments.requiredFields'));
      return;
    }

    this.isSubmitting = true;
    this.departmentService.addDepartment(this.newDepartment).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('departments.addSuccess'));
        this.newDepartment = this.emptyNewDepartment();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
  }

  onEdit(department: Department)
  {
    this.editingDepartment = { ...department };
  }

  onUpdate()
  {
    if (!this.editingDepartment) return;

    if (!this.editingDepartment.nameEn.trim() || !this.editingDepartment.nameAr.trim()) {
      this.snackbar.showError(this.translate.instant('departments.updateRequiredFields'));
      return;
    }

    this.isSubmitting = true;
    this.departmentService.updateDepartment(this.editingDepartment).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: (response) => {
        console.log(response.message);
        this.snackbar.showSuccess(this.translate.instant('departments.updateSuccess'));
        this.editingDepartment = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Update failed.', error);
        if (error.status === 404) {
          this.snackbar.showError(error.error?.message || this.translate.instant('departments.notFound'));
        }
      }
    });
  }

  onDelete(department: Department): void
  {
    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirmDeleteTitle'),
        message: this.translate.instant('common.confirmDeleteMessage', { name: department.nameEn })
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.deletingId = department.id;
      this.departmentService.deleteDepartment(department.id).pipe(
        finalize(() => this.deletingId = null)
      ).subscribe({
        next: (response) => {
          console.log(response.message);
          this.snackbar.showSuccess(this.translate.instant('departments.deleteSuccess'));
          this.loadData();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('API Error: Delete failed.', error);
          this.snackbar.showError(error.error?.message || this.translate.instant('departments.deleteError'));
        }
      });
    });
  }

  statusClass(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'pill-active' : 'pill-inactive';
  }

  statusLabelKey(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'common.statusActive' : 'common.statusInactive';
  }

  toggleStatus(item: Department): void
  {
    const newStatus = item.status === 'Active' ? 'Inactive' : 'Active';
    const updated: Department = { ...item, status: newStatus };

    this.togglingId = item.id;
    this.departmentService.updateDepartment(updated).pipe(
      finalize(() => this.togglingId = null)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('departments.statusUpdateSuccess'));
        this.loadData();
      },
      error: (error) => {
        console.error('API Error: Status toggle failed.', error);
        this.snackbar.showError(this.translate.instant('departments.statusUpdateError'));
      }
    });
  }
}