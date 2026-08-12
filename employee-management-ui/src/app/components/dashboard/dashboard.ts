import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';
import { DashboardService, DashboardSummary, DepartmentEmployeeCount } from '../../services/dashboard';
import { NavbarComponent } from '../navbar/navbar';
import { EmployeeBarChartComponent, BarChartDatum } from '../employee-bar-chart/employee-bar-chart';
import { SnackbarService } from '../../services/snackbar';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../services/language';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule, MatProgressBarModule, MatIconModule,
    NavbarComponent, EmployeeBarChartComponent, TranslatePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit, OnDestroy
{
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private snackbar = inject(SnackbarService);
  private languageService = inject(LanguageService);

  private destroy$ = new Subject<void>();

  // Summary cards
  summary: DashboardSummary | null = null;
  isLoadingSummary = false;
  summaryError = false;

  // Employees by department chart
  private departmentCounts: DepartmentEmployeeCount[] = [];
  departmentChartData: BarChartDatum[] = [];
  isLoadingDepartmentChart = false;
  departmentChartError = false;

  departmentCategoryLabel = '';
  employeesValueLabel = '';
  noChartDataLabel = '';

  get hasNoSummaryData(): boolean
  {
    if (!this.summary) return false;

    return this.summary.totalDepartments === 0 &&
      this.summary.totalPositions === 0 &&
      this.summary.totalActiveEmployees === 0 &&
      this.summary.totalMaleEmployees === 0 &&
      this.summary.totalFemaleEmployees === 0;
  }

  ngOnInit(): void
  {
    this.refreshChartLabels();

    // refresh on language change
    this.translate.onLangChange.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.refreshChartLabels();
      this.departmentChartData = this.mapDepartmentData(this.departmentCounts);
    });

    this.loadSummary();
    this.loadDepartmentChart();
  }

  ngOnDestroy(): void
  {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSummary(): void
  {
    this.isLoadingSummary = true;
    this.summaryError = false;

    this.dashboardService.getSummary().subscribe({
      next: (result) => {
        this.summary = result;
        this.isLoadingSummary = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch dashboard summary.', error);
        this.summaryError = true;
        this.isLoadingSummary = false;
        this.snackbar.showError(this.translate.instant('dashboard.fetchError'));
        this.cdr.detectChanges();
      }
    });
  }

  loadDepartmentChart(): void
  {
    this.isLoadingDepartmentChart = true;
    this.departmentChartError = false;

    this.dashboardService.getEmployeesByDepartment().subscribe({
      next: (result) => {
        this.departmentCounts = result;
        this.departmentChartData = this.mapDepartmentData(result);
        this.isLoadingDepartmentChart = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch employees-by-department chart data.', error);
        this.departmentChartError = true;
        this.isLoadingDepartmentChart = false;
        this.snackbar.showError(this.translate.instant('dashboard.departmentChartError'));
        this.cdr.detectChanges();
      }
    });
  }

  private mapDepartmentData(items: DepartmentEmployeeCount[]): BarChartDatum[]
  {
    const isArabic = this.languageService.getCurrentLang() === 'ar';
    return items.map(item => ({
      name: isArabic ? item.departmentNameAr : item.departmentNameEn,
      value: item.employeeCount
    }));
  }

  private refreshChartLabels(): void
  {
    this.departmentCategoryLabel = this.translate.instant('dashboard.departmentLabel');
    this.employeesValueLabel = this.translate.instant('dashboard.employeesLabel');
    this.noChartDataLabel = this.translate.instant('dashboard.noChartData');
  }
}