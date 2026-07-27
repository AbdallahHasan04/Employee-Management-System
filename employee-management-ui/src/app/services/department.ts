import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Department
{
    id: number;
    departmentCode: string;
    nameEn: string;
    nameAr: string;
    description: string | null;
    status: string;
    employeeCount: number;
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
}

export type NewDepartment = Pick<Department, 'departmentCode' | 'nameEn' | 'nameAr' | 'description'>;

export interface CreateDepartmentResponse
{
    message: string;
    department: Department;
}

export interface PagedResult<T>
{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface DepartmentQueryParams
{
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
    search?: string;
}

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/departments';

    getDepartments(params: DepartmentQueryParams): Observable<PagedResult<Department>>
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

        return this.http.get<PagedResult<Department>>(this.apiUrl, { params: httpParams });
    }

    addDepartment(department: NewDepartment): Observable<CreateDepartmentResponse>
    {
        return this.http.post<CreateDepartmentResponse>(this.apiUrl, department);
    }

    updateDepartment(department: Department): Observable<any>
    {
        return this.http.put(this.apiUrl, department);
    }

    deleteDepartment(id: number): Observable<any>
    {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}