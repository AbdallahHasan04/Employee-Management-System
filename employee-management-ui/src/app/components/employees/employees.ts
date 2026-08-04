import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, finalize } from 'rxjs';
import { EmployeeService, Employee, NewEmployee } from '../../services/employee';
import { DepartmentService, Department } from '../../services/department';
import { PositionService, Position } from '../../services/position';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

const MAX_PHOTO_SIZE_BYTES = 2 * 1024 * 1024;
const ALLOWED_PHOTO_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'app-employees',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatIconModule, MatProgressSpinnerModule,
    MatPaginatorModule, MatSortModule, MatProgressBarModule,
    NavbarComponent, TranslatePipe,
  ],
  templateUrl: './employees.html',
  styleUrl: './employees.css',
})
export class EmployeesComponent implements OnInit, OnDestroy
{
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private positionService = inject(PositionService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private dialog = inject(MatDialog);
  private snackbar = inject(SnackbarService);

  employees: Employee[] = [];
  departments: Department[] = [];
  positions: Position[] = [];
  displayedColumns: string[] = [
    'expand', 'photo', 'employeeNo', 'nameEn', 'nameAr', 'departmentName', 'positionName', 'username', 'nationalNo',
    'gender', 'birthdate', 'mobileNumber', 'email', 'startWorkingDate',
    'status', 'actions'
  ];

  newEmployee: NewEmployee = this.emptyNewEmployee();
  editingEmployee: Employee | null = null;
  private originalEditingEmployee: Employee | null = null;

  lastCreatedUsername: string | null = null;
  lastGeneratedPassword: string | null = null;

  isSubmitting = false;
  deletingId: number | null = null;
  togglingId: number | null = null;
  expandedId: number | null = null;
  isLoading = false;

  selectedPhotoFile: File | null = null;
  selectedPhotoPreviewUrl: string | null = null;
  isUploadingPhoto = false;

  searchTerm = '';
  private search$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  sortActive = 'employeeNo';
  sortDirection: 'asc' | 'desc' | '' = 'asc';

  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 25, 50];

  ngOnInit()
  {
    this.loadDepartments();
    this.loadPositions();

    this.search$.pipe(
      debounceTime(0),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.pageIndex = 0;
      this.loadData();
    });

    this.loadData();
  }

  ngOnDestroy(): void
  {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearSelectedPhoto();
  }

  private emptyNewEmployee(): NewEmployee
  {
    return {
      employeeNo: '', nameEn: '', nameAr: '', username: '',
      birthdate: null, nationalNo: '', gender: '',
      mobileNumber: null, email: null, startWorkingDate: null,
      departmentId: null, positionId: null
    };
  }

  onSearchChange(term: string): void
  {
    this.search$.next(term);
  }

  onSortChange(sort: Sort): void
  {
    this.sortActive = sort.active;
    this.sortDirection = sort.direction;
    this.pageIndex = 0;
    this.loadData();
  }

  onPageChange(event: PageEvent): void
  {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  loadData()
  {
    this.isLoading = true;
    this.employeeService.getEmployees({
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortDirection ? this.sortActive : undefined,
      sortDescending: this.sortDirection === 'desc',
      search: this.searchTerm || undefined
    }).subscribe({
      next: (result) => {
        this.employees = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        this.snackbar.showError(this.translate.instant('employees.fetchError'));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadDepartments()
  {
    this.departmentService.getDepartments({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.departments = result.items;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch departments.', error);
      }
    });
  }

  loadPositions()
  {
    this.positionService.getPositions({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.positions = result.items;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch positions.', error);
      }
    });
  }

  getPhotoUrl(path: string | null | undefined): string | null
  {
    return this.employeeService.getPhotoUrl(path);
  }

  private validatePhotoFile(file: File): boolean
  {
    if (!ALLOWED_PHOTO_TYPES.includes(file.type)) {
      this.snackbar.showError(this.translate.instant('employees.invalidPhotoType'));
      return false;
    }
    if (file.size > MAX_PHOTO_SIZE_BYTES) {
      this.snackbar.showError(this.translate.instant('employees.photoTooLarge'));
      return false;
    }
    return true;
  }

  onNewPhotoSelected(event: Event): void
  {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!this.validatePhotoFile(file)) {
      input.value = '';
      return;
    }

    this.clearSelectedPhoto();
    this.selectedPhotoFile = file;
    this.selectedPhotoPreviewUrl = URL.createObjectURL(file);
  }

  clearSelectedPhoto(): void
  {
    if (this.selectedPhotoPreviewUrl) {
      URL.revokeObjectURL(this.selectedPhotoPreviewUrl);
    }
    this.selectedPhotoFile = null;
    this.selectedPhotoPreviewUrl = null;
  }

  onEditPhotoSelected(event: Event): void
  {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.editingEmployee) return;

    if (!this.validatePhotoFile(file)) {
      input.value = '';
      return;
    }

    this.isUploadingPhoto = true;
    this.employeeService.uploadPhoto(this.editingEmployee.id, file).pipe(
      finalize(() => {
        this.isUploadingPhoto = false;
        input.value = '';
      })
    ).subscribe({
      next: (response) => {
        this.snackbar.showSuccess(this.translate.instant('employees.photoUploadSuccess'));
        this.editingEmployee = response.employee;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Photo upload failed.', error);
        this.snackbar.showError(error.error?.message || this.translate.instant('employees.photoUploadError'));
      }
    });
  }

  removeEditingPhoto(): void
  {
    if (!this.editingEmployee) return;

    this.isUploadingPhoto = true;
    this.employeeService.removePhoto(this.editingEmployee.id).pipe(
      finalize(() => this.isUploadingPhoto = false)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('employees.photoRemoveSuccess'));
        if (this.editingEmployee) {
          this.editingEmployee = { ...this.editingEmployee, profileImagePath: null };
        }
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Photo removal failed.', error);
        this.snackbar.showError(this.translate.instant('employees.photoRemoveError'));
      }
    });
  }

  onAdd(): void
  {
    const requiredStrings = [this.newEmployee.employeeNo, this.newEmployee.nameEn, this.newEmployee.username, this.newEmployee.nationalNo];
    if (requiredStrings.some(f => !f?.trim())) {
      this.snackbar.showError(this.translate.instant('employees.requiredFields'));
      return;
    }

    if (!this.newEmployee.departmentId) {
      this.snackbar.showError(this.translate.instant('employees.departmentRequired'));
      return;
    }

    if (!this.newEmployee.positionId) {
      this.snackbar.showError(this.translate.instant('employees.positionRequired'));
      return;
    }

    this.isSubmitting = true;
    this.employeeService.addEmployee(this.newEmployee).subscribe({
      next: (response) => {
        this.lastCreatedUsername = response.employee.username;
        this.lastGeneratedPassword = response.employee.generatedPassword ?? null;

        const newId = response.employee.id;
        const photoFile = this.selectedPhotoFile;

        if (photoFile) {
          this.employeeService.uploadPhoto(newId, photoFile).pipe(
            finalize(() => {
              this.isSubmitting = false;
              this.clearSelectedPhoto();
            })
          ).subscribe({
            next: () => {
              this.newEmployee = this.emptyNewEmployee();
              this.loadData();
              this.cdr.detectChanges();
            },
            error: (error) => {
              console.error('API Error: Photo upload failed.', error);
              this.snackbar.showError(this.translate.instant('employees.photoUploadError'));
              this.newEmployee = this.emptyNewEmployee();
              this.loadData();
              this.cdr.detectChanges();
            }
          });
        } else {
          this.isSubmitting = false;
          this.newEmployee = this.emptyNewEmployee();
          this.loadData();
          this.cdr.detectChanges();
        }
      },
      error: (error) => {
        console.error('API Error: Add failed.', error);
        this.isSubmitting = false;
      }
    });
  }

  onEdit(employee: Employee)
  {
    this.editingEmployee = { ...employee };
    this.originalEditingEmployee = { ...employee };
  }

  onCancelEdit(): void
  {
    if (!this.editingEmployee) return;

    const hasChanges = JSON.stringify(this.editingEmployee) !== JSON.stringify(this.originalEditingEmployee);
    if (!hasChanges) {
      this.editingEmployee = null;
      return;
    }

    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.discardChangesTitle'),
        message: this.translate.instant('common.discardChangesMessage'),
        confirmLabel: this.translate.instant('common.discardChanges')
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.editingEmployee = null;
        this.cdr.detectChanges();
      }
    });
  }

  onUpdate()
  {
    if (!this.editingEmployee) return;

    if (!this.editingEmployee.nameEn.trim() || !this.editingEmployee.nationalNo.trim() || !this.editingEmployee.departmentId) {
      this.snackbar.showError(this.translate.instant('employees.updateRequiredFields'));
      return;
    }

    if (!this.editingEmployee.positionId) {
      this.snackbar.showError(this.translate.instant('employees.positionRequired'));
      return;
    }

    this.isSubmitting = true;
    this.employeeService.updateEmployee(this.editingEmployee).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: (response) => {
        console.log(response.message);
        this.snackbar.showSuccess(this.translate.instant('employees.updateSuccess'));
        this.editingEmployee = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Update failed.', error);
        if (error.status === 404) {
          this.snackbar.showError(error.error?.message || this.translate.instant('employees.notFound'));
        }
      }
    });
  }

  onDelete(employee: Employee): void
  {
    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirmDeleteTitle'),
        message: this.translate.instant('common.confirmDeleteMessage', { name: employee.nameEn })
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.deletingId = employee.id;
      this.employeeService.deleteEmployee(employee.id).pipe(
        finalize(() => this.deletingId = null)
      ).subscribe({
        next: (response) => {
          console.log(response.message);
          this.snackbar.showSuccess(this.translate.instant('employees.deleteSuccess'));
          this.loadData();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('API Error: Delete failed.', error);
          if (error.status === 404) {
            this.snackbar.showError(error.error?.message || this.translate.instant('employees.notFound'));
          }
        }
      });
    });
  }

  dismissCredentials()
  {
    this.lastCreatedUsername = null;
    this.lastGeneratedPassword = null;
  }

  statusClass(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'pill-active' : 'pill-inactive';
  }

  statusLabelKey(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'common.statusActive' : 'common.statusInactive';
  }

  genderLabelKey(gender: string): string
  {
    return gender === 'Female' ? 'employees.genderFemale' : 'employees.genderMale';
  }

  toggleStatus(item: Employee): void
  {
    const newStatus = item.status === 'Active' ? 'Inactive' : 'Active';
    const updated: Employee = { ...item, status: newStatus };

    this.togglingId = item.id;
    this.employeeService.updateEmployee(updated).pipe(
      finalize(() => this.togglingId = null)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('employees.statusUpdateSuccess'));
        this.loadData();
      },
      error: (error) => {
        console.error('API Error: Status toggle failed.', error);
        this.snackbar.showError(this.translate.instant('employees.statusUpdateError'));
      }
    });
  }

  toggleExpand(item: Employee): void
  {
    this.expandedId = this.expandedId === item.id ? null : item.id;
  }
}