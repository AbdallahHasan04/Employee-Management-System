import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardSummary
{
    totalDepartments: number;
    totalPositions: number;
    totalActiveEmployees: number;
    totalMaleEmployees: number;
    totalFemaleEmployees: number;
}

export interface DepartmentEmployeeCount
{
    departmentNameEn: string;
    departmentNameAr: string;
    employeeCount: number;
}

export interface PositionEmployeeCount
{
    positionNameEn: string;
    positionNameAr: string;
    employeeCount: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/dashboard';

    getSummary(): Observable<DashboardSummary>
    {
        return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`);
    }

    getEmployeesByDepartment(): Observable<DepartmentEmployeeCount[]>
    {
        return this.http.get<DepartmentEmployeeCount[]>(`${this.apiUrl}/employees-by-department`);
    }

    getEmployeesByPosition(): Observable<PositionEmployeeCount[]>
    {
        return this.http.get<PositionEmployeeCount[]>(`${this.apiUrl}/employees-by-position`);
    }
}