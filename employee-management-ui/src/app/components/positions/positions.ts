import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, finalize } from 'rxjs';
import { PositionService, Position, NewPosition } from '../../services/position';
import { NavbarComponent } from '../navbar/navbar';
import { ConfirmDialogComponent, ConfirmDialogData } from '../confirm-dialog/confirm-dialog';
import { SnackbarService } from '../../services/snackbar';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-positions',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatProgressSpinnerModule,
    MatPaginatorModule, MatSortModule, MatProgressBarModule,
    NavbarComponent, TranslatePipe,
  ],
  templateUrl: './positions.html',
  styleUrl: './positions.css',
})
export class PositionsComponent implements OnInit, OnDestroy
{
  private positionService = inject(PositionService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private dialog = inject(MatDialog);
  private snackbar = inject(SnackbarService);

  positions: Position[] = [];
  displayedColumns: string[] = ['expand', 'nameEn', 'nameAr', 'actions'];

  newPosition: NewPosition = this.emptyNewPosition();
  editingPosition: Position | null = null;

  isSubmitting = false;
  deletingId: number | null = null;
  expandedId: number | null = null;
  isLoading = false;

  searchTerm = '';
  private search$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  sortActive = 'nameEn';
  sortDirection: 'asc' | 'desc' | '' = 'asc';

  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 25, 50];

  ngOnInit()
  {
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

  private emptyNewPosition(): NewPosition
  {
    return { nameEn: '', nameAr: '' };
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
    this.positionService.getPositions({
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortDirection ? this.sortActive : undefined,
      sortDescending: this.sortDirection === 'desc',
      search: this.searchTerm || undefined
    }).subscribe({
      next: (result) => {
        this.positions = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        this.snackbar.showError(this.translate.instant('positions.fetchError'));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onAdd(): void
  {
    const required = [this.newPosition.nameEn, this.newPosition.nameAr];
    if (required.some(f => !f?.trim())) {
      this.snackbar.showError(this.translate.instant('positions.requiredFields'));
      return;
    }

    this.isSubmitting = true;
    this.positionService.addPosition(this.newPosition).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: () => {
        this.snackbar.showSuccess(this.translate.instant('positions.addSuccess'));
        this.newPosition = this.emptyNewPosition();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
  }

  onEdit(position: Position)
  {
    this.editingPosition = { ...position };
  }

  onUpdate()
  {
    if (!this.editingPosition) return;

    if (!this.editingPosition.nameEn.trim() || !this.editingPosition.nameAr.trim()) {
      this.snackbar.showError(this.translate.instant('positions.updateRequiredFields'));
      return;
    }

    this.isSubmitting = true;
    this.positionService.updatePosition(this.editingPosition).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: (response) => {
        console.log(response.message);
        this.snackbar.showSuccess(this.translate.instant('positions.updateSuccess'));
        this.editingPosition = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Update failed.', error);
        if (error.status === 404) {
          this.snackbar.showError(error.error?.message || this.translate.instant('positions.notFound'));
        }
      }
    });
  }

  onDelete(position: Position): void
  {
    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      data: {
        title: this.translate.instant('common.confirmDeleteTitle'),
        message: this.translate.instant('common.confirmDeleteMessage', { name: position.nameEn })
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.deletingId = position.id;
      this.positionService.deletePosition(position.id).pipe(
        finalize(() => this.deletingId = null)
      ).subscribe({
        next: (response) => {
          console.log(response.message);
          this.snackbar.showSuccess(this.translate.instant('positions.deleteSuccess'));
          this.loadData();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('API Error: Delete failed.', error);
          this.snackbar.showError(error.error?.message || this.translate.instant('positions.deleteError'));
        }
      });
    });
  }

  toggleExpand(item: Position): void
  {
    this.expandedId = this.expandedId === item.id ? null : item.id;
  }
}