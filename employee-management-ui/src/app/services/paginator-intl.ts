import { Injectable, inject } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CustomPaginatorIntl extends MatPaginatorIntl
{
  private translate = inject(TranslateService);
  override changes = new Subject<void>();

  constructor()
  {
    super();
    this.translate.onLangChange.subscribe(() => this.updateLabels());
    this.updateLabels();
  }

  override getRangeLabel = (page: number, pageSize: number, length: number): string =>
  {
    if (length === 0 || pageSize === 0) {
      return `0 / ${length}`;
    }
    const startIndex = page * pageSize;
    const endIndex = startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;
    return `${startIndex + 1} – ${endIndex} / ${length}`;
  };

  private updateLabels(): void
  {
    this.itemsPerPageLabel = this.translate.instant('common.itemsPerPage');
    this.nextPageLabel = this.translate.instant('common.nextPage');
    this.previousPageLabel = this.translate.instant('common.previousPage');
    this.firstPageLabel = this.translate.instant('common.firstPage');
    this.lastPageLabel = this.translate.instant('common.lastPage');
    this.changes.next();
  }
}