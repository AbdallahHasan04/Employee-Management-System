import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, finalize } from 'rxjs';
import { EmployeePositionService, EmployeePosition, AssignPosition } from '../../services/employee-position';
import { EmployeeService, Employee } from '../../services/employee';
import { PositionService, Position } from '../../services/position';
import { NavbarComponent } from '../navbar/navbar';
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
  selector: 'app-position-history',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatIconModule, MatProgressSpinnerModule,
    MatPaginatorModule, MatSortModule, MatProgressBarModule,
    NavbarComponent, TranslatePipe,
  ],
  templateUrl: './position-history.html',
  styleUrl: './position-history.css',
})
export class PositionHistoryComponent implements OnInit, OnDestroy
{
  private employeePositionService = inject(EmployeePositionService);
  private employeeService = inject(EmployeeService);
  private positionService = inject(PositionService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private snackbar = inject(SnackbarService);

  history: EmployeePosition[] = [];
  employees: Employee[] = [];
  positions: Position[] = [];
  displayedColumns: string[] = ['expand', 'employeeName', 'positionName', 'startDate', 'endDate'];

  newAssignment: AssignPosition = this.emptyAssignment();

  isSubmitting = false;
  expandedId: number | null = null;
  isLoading = false;

  searchTerm = '';
  private search$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  sortActive = 'startDate';
  sortDirection: 'asc' | 'desc' | '' = 'desc';

  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 25, 50];

  ngOnInit()
  {
    this.loadEmployees();
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
  }

  private emptyAssignment(): AssignPosition
  {
    return { employeeId: null, positionId: null, startDate: '' };
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
    this.employeePositionService.getHistory({
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortDirection ? this.sortActive : undefined,
      sortDescending: this.sortDirection === 'desc',
      search: this.searchTerm || undefined
    }).subscribe({
      next: (result) => {
        this.history = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        this.snackbar.showError(this.translate.instant('positionHistory.fetchError'));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadEmployees()
  {
    this.employeeService.getEmployees({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.employees = result.items;
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Could not fetch employees.', error)
    });
  }

  loadPositions()
  {
    this.positionService.getPositions({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.positions = result.items;
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Could not fetch positions.', error)
    });
  }

  onAssign(): void
  {
    if (!this.newAssignment.employeeId || !this.newAssignment.positionId || !this.newAssignment.startDate) {
      this.snackbar.showError(this.translate.instant('positionHistory.requiredFields'));
      return;
    }

    this.isSubmitting = true;
    this.employeePositionService.assignPosition(this.newAssignment).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('positionHistory.assignSuccess'));
        this.newAssignment = this.emptyAssignment();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Assign failed.', error);
        this.snackbar.showError(error.error?.message || this.translate.instant('positionHistory.assignError'));
      }
    });
  }

  toggleExpand(item: EmployeePosition): void
  {
    this.expandedId = this.expandedId === item.id ? null : item.id;
  }
}