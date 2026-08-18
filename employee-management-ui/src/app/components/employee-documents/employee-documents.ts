import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, ViewContainerRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, finalize } from 'rxjs';
import { EmployeeDocumentService, EmployeeDocument, NewEmployeeDocument } from '../../services/employee-document';
import { EmployeeService, Employee } from '../../services/employee';
import { NavbarComponent } from '../navbar/navbar';
import { ConfirmDialogComponent, ConfirmDialogData } from '../confirm-dialog/confirm-dialog';
import { SnackbarService } from '../../services/snackbar';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../services/language';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-employee-documents',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatIconModule, MatProgressSpinnerModule,
    MatPaginatorModule, MatSortModule, MatProgressBarModule,
    MatTooltipModule, NavbarComponent, TranslatePipe,
  ],
  templateUrl: './employee-documents.html',
  styleUrl: './employee-documents.css',
})
export class EmployeeDocumentsComponent implements OnInit, OnDestroy
{
  private employeeDocumentService = inject(EmployeeDocumentService);
  private employeeService = inject(EmployeeService);
  private cdr = inject(ChangeDetectorRef);
  private translate = inject(TranslateService);
  private snackbar = inject(SnackbarService);
  private dialog = inject(MatDialog);
  private viewContainerRef = inject(ViewContainerRef);
  private languageService = inject(LanguageService);

  documents: EmployeeDocument[] = [];
  employees: Employee[] = [];
  displayedColumns: string[] = ['employeeName', 'documentName', 'issueDate', 'expiryDate', 'actions'];

  newDocument: NewEmployeeDocument = this.emptyNewDocument();
  selectedFileName: string | null = null;

  isSubmitting = false;
  downloadingId: number | null = null;
  deletingId: number | null = null;
  isLoading = false;

  searchTerm = '';
  private search$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  sortActive = 'creationDate';
  sortDirection: 'asc' | 'desc' | '' = '';

  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 25, 50];

  get today(): string
  {
    return new Date().toISOString().split('T')[0];
  }

  ngOnInit()
  {
    this.loadEmployees();

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

  private emptyNewDocument(): NewEmployeeDocument
  {
    return { employeeId: null, documentName: '', issueDate: '', expiryDate: null, notes: null, attachment: null };
  }

  isExpired(doc: EmployeeDocument): boolean
  {
    if (!doc.expiryDate) return false;
    return doc.expiryDate < this.today;
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

  loadData(): void
  {
    this.isLoading = true;
    this.employeeDocumentService.getDocuments({
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortDirection ? this.sortActive : undefined,
      sortDescending: this.sortDirection === 'desc',
      search: this.searchTerm || undefined
    }).subscribe({
      next: (result) => {
        this.documents = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        this.snackbar.showError(this.translate.instant('employeeDocuments.fetchError'));
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadEmployees(): void
  {
    this.employeeService.getEmployees({ pageNumber: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.employees = result.items;
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Could not fetch employees.', error)
    });
  }

  onFileSelected(event: Event): void
  {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.newDocument.attachment = file;
    this.selectedFileName = file.name;
  }

  onAdd(): void
  {
    if (!this.newDocument.employeeId) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.employeeRequired'));
      return;
    }

    if (!this.newDocument.documentName?.trim() || !this.newDocument.issueDate) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.requiredFields'));
      return;
    }

    if (this.newDocument.issueDate > this.today) {
      this.snackbar.showError(this.translate.instant('common.futureDateError'));
      return;
    }

    if (!this.newDocument.attachment) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.attachmentRequired'));
      return;
    }

    if (this.newDocument.expiryDate && this.newDocument.expiryDate < this.newDocument.issueDate) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.expiryBeforeIssueError'));
      return;
    }

    this.isSubmitting = true;
    this.employeeDocumentService.uploadDocument(this.newDocument).pipe(
      finalize(() => this.isSubmitting = false)
    ).subscribe({
      next: (response) => {
        console.log(response.message);
        this.snackbar.showSuccess(this.translate.instant('employeeDocuments.addSuccess'));
        this.newDocument = this.emptyNewDocument();
        this.selectedFileName = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Upload failed.', error);
        this.snackbar.showError(error.error?.message || this.translate.instant('employeeDocuments.addError'));
      }
    });
  }

  onDownload(doc: EmployeeDocument): void
  {
    if (this.isExpired(doc)) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.downloadExpiredError'));
      return;
    }

    this.downloadingId = doc.id;
    this.employeeDocumentService.downloadDocument(doc.id).pipe(
      finalize(() => this.downloadingId = null)
    ).subscribe({
      next: (response) => {
        const blob = response.body;
        if (!blob) {
          this.snackbar.showError(this.translate.instant('employeeDocuments.downloadError'));
          return;
        }

        const fileName = this.extractFileName(response.headers.get('content-disposition')) || doc.documentName;
        const blobUrl = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(blobUrl);
      },
      error: (error: HttpErrorResponse) => {
        console.error('API Error: Download failed.', error);
        this.parseBlobError(error).then(message => {
          this.snackbar.showError(message || this.translate.instant('employeeDocuments.downloadError'));
        });
      }
    });
  }

  onDelete(doc: EmployeeDocument): void
  {
    if (!this.isExpired(doc)) {
      this.snackbar.showError(this.translate.instant('employeeDocuments.deleteNotExpiredError'));
      return;
    }

    const dialogRef = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      viewContainerRef: this.viewContainerRef,
      direction: this.languageService.dir(),
      data: {
        title: this.translate.instant('common.confirmDeleteTitle'),
        message: this.translate.instant('common.confirmDeleteMessage', { name: doc.documentName })
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.deletingId = doc.id;
      this.employeeDocumentService.deleteDocument(doc.id).pipe(
        finalize(() => this.deletingId = null)
      ).subscribe({
        next: (response) => {
          console.log(response.message);
          this.snackbar.showSuccess(this.translate.instant('employeeDocuments.deleteSuccess'));
          this.loadData();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('API Error: Delete failed.', error);
          this.snackbar.showError(error.error?.message || this.translate.instant('employeeDocuments.deleteError'));
        }
      });
    });
  }

  private async parseBlobError(error: HttpErrorResponse): Promise<string | null>
  {
    if (error.error instanceof Blob) {
      try {
        const text = await error.error.text();
        const parsed = JSON.parse(text);
        return parsed?.message ?? null;
      } catch {
        return null;
      }
    }
    return error.error?.message ?? null;
  }

  private extractFileName(contentDisposition: string | null): string | null
  {
    if (!contentDisposition) return null;
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(contentDisposition);
    return match ? decodeURIComponent(match[1]) : null;
  }
}