import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Position
{
    id: number;
    nameEn: string;
    nameAr: string;
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
}

export type NewPosition = Pick<Position, 'nameEn' | 'nameAr'>;

export interface CreatePositionResponse
{
    message: string;
    position: Position;
}

export interface PagedResult<T>
{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface PositionQueryParams
{
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
    search?: string;
}

@Injectable({ providedIn: 'root' })
export class PositionService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/positions';

    getPositions(params: PositionQueryParams): Observable<PagedResult<Position>>
    {
        let httpParams = new HttpParams()
            .set('pageNumber', params.pageNumber)
            .set('pageSize', params.pageSize);

        if (params.sortBy) {
            httpParams = httpParams
                .set('sortBy', params.sortBy)
                .set('sortDescending', params.sortDescending ?? false);
        }
        if (params.search) {
            httpParams = httpParams.set('search', params.search);
        }

        return this.http.get<PagedResult<Position>>(this.apiUrl, { params: httpParams });
    }

    addPosition(position: NewPosition): Observable<CreatePositionResponse>
    {
        return this.http.post<CreatePositionResponse>(this.apiUrl, position);
    }

    updatePosition(position: Position): Observable<any>
    {
        return this.http.put(this.apiUrl, position);
    }

    deletePosition(id: number): Observable<any>
    {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}