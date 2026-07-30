import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EmployeePosition
{
    id: number;
    employeeId: number;
    employeeName: string | null;
    positionId: number;
    positionName: string | null;
    startDate: string;
    endDate: string | null;
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
}

export interface AssignPosition
{
    employeeId: number | null;
    positionId: number | null;
    startDate: string;
}

export interface AssignPositionResponse
{
    message: string;
    employeePosition: EmployeePosition;
}

export interface PagedResult<T>
{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface EmployeePositionQueryParams
{
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
    search?: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeePositionService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/employeepositions';

    getHistory(params: EmployeePositionQueryParams): Observable<PagedResult<EmployeePosition>>
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

        return this.http.get<PagedResult<EmployeePosition>>(this.apiUrl, { params: httpParams });
    }

    assignPosition(dto: AssignPosition): Observable<AssignPositionResponse>
    {
        return this.http.post<AssignPositionResponse>(this.apiUrl, dto);
    }
}