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
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private dialog = inject(MatDialog);
  private snackbar = inject(SnackbarService);

  employees: Employee[] = [];
  departments: Department[] = [];
  displayedColumns: string[] = [
    'expand', 'employeeNo', 'nameEn', 'nameAr', 'departmentName', 'username', 'nationalNo',
    'gender', 'birthdate', 'mobileNumber', 'email', 'startWorkingDate',
    'status', 'actions'
  ];

  newEmployee: NewEmployee = this.emptyNewEmployee();
  editingEmployee: Employee | null = null;

  lastCreatedUsername: string | null = null;
  lastGeneratedPassword: string | null = null;

  isSubmitting = false;
  deletingId: number | null = null;
  togglingId: number | null = null;
  expandedId: number | null = null;
  isLoading = false;

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

    this.search$.pipe(
      debounceTime(400),
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
  }

  private emptyNewEmployee(): NewEmployee
  {
    return {
      employeeNo: '', nameEn: '', nameAr: '', username: '',
      birthdate: null, nationalNo: '', gender: '',
      mobileNumber: null, email: null, startWorkingDate: null,
      departmentId: null
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
    // pageSize:1000 here is deliberate — this populates the Department dropdown in the
    // add/edit form, which needs effectively "all" departments, not a paginated page of them.
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

    this.isSubmitting = true;
    this.employeeService.addEmployee(this.newEmployee).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: (response) => {
        this.lastCreatedUsername = response.employee.username;
        this.lastGeneratedPassword = response.employee.generatedPassword ?? null;
        this.newEmployee = this.emptyNewEmployee();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
  }

  onEdit(employee: Employee)
  {
    this.editingEmployee = { ...employee };
  }

  onUpdate()
  {
    if (!this.editingEmployee) return;

    if (!this.editingEmployee.nameEn.trim() || !this.editingEmployee.nationalNo.trim() || !this.editingEmployee.departmentId) {
      this.snackbar.showError(this.translate.instant('employees.updateRequiredFields'));
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