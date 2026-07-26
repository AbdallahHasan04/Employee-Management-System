import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/departments';

    getDepartments(): Observable<Department[]>
    {
        return this.http.get<Department[]>(this.apiUrl);
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