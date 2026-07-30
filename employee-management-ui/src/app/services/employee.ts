import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Employee
{
    id: number;
    employeeNo: string;
    nameEn: string;
    nameAr: string;
    username: string;
    birthdate: string | null;
    nationalNo: string;
    gender: string;
    status: string;
    mobileNumber: string | null;
    email: string | null;
    startWorkingDate: string | null;
    departmentId: number;
    departmentName?: string;
    positionId: number | null;
    positionName?: string;
    profileImagePath: string | null;
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
    generatedPassword?: string;
}

export type NewEmployee = Pick<Employee,
  'employeeNo' | 'nameEn' | 'nameAr' | 'username' | 'birthdate' |
  'nationalNo' | 'gender' | 'mobileNumber' | 'email' | 'startWorkingDate'
> & { departmentId: number | null; positionId: number | null };

export interface CreateEmployeeResponse
{
    message: string;
    employee: Employee;
}

export interface PhotoResponse
{
    message: string;
    employee: Employee;
}

export interface PagedResult<T>
{
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface EmployeeQueryParams
{
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
    search?: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/employees';
  private photoBaseUrl = 'https://localhost:7038/';

    getEmployees(params: EmployeeQueryParams): Observable<PagedResult<Employee>>
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

        return this.http.get<PagedResult<Employee>>(this.apiUrl, { params: httpParams });
    }

    addEmployee(employee: NewEmployee): Observable<CreateEmployeeResponse>
    {
        return this.http.post<CreateEmployeeResponse>(this.apiUrl, employee);
    }

    updateEmployee(employee: Employee): Observable<any>
    {
        return this.http.put(this.apiUrl, employee);
    }

    deleteEmployee(id: number): Observable<any>
    {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    uploadPhoto(id: number, file: File): Observable<PhotoResponse>
    {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<PhotoResponse>(`${this.apiUrl}/${id}/photo`, formData);
    }

    removePhoto(id: number): Observable<any>
    {
        return this.http.delete(`${this.apiUrl}/${id}/photo`);
    }

    getPhotoUrl(path: string | null | undefined): string | null
    {
        return path ? `${this.photoBaseUrl}${path}` : null;
    }
}