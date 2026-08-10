import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { DashboardService, DashboardSummary } from '../../services/dashboard';
import { NavbarComponent } from '../navbar/navbar';
import { SnackbarService } from '../../services/snackbar';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule, MatProgressBarModule, MatIconModule,
    NavbarComponent, TranslatePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit
{
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private snackbar = inject(SnackbarService);

  summary: DashboardSummary | null = null;
  isLoading = false;
  hasError = false;

  get hasNoData(): boolean
  {
    if (!this.summary) return false;

    return this.summary.totalDepartments === 0 &&
      this.summary.totalPositions === 0 &&
      this.summary.totalActiveEmployees === 0 &&
      this.summary.totalMaleEmployees === 0 &&
      this.summary.totalFemaleEmployees === 0;
  }

  ngOnInit()
  {
    this.loadSummary();
  }

  loadSummary(): void
  {
    this.isLoading = true;
    this.hasError = false;

    this.dashboardService.getSummary().subscribe({
      next: (result) => {
        this.summary = result;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch dashboard summary.', error);
        this.hasError = true;
        this.isLoading = false;
        this.snackbar.showError(this.translate.instant('dashboard.fetchError'));
        this.cdr.detectChanges();
      }
    });
  }
}