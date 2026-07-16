import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    createdBy: string | null;
    creationDate: string;
    modifiedBy: string | null;
    modificationDate: string | null;
    generatedPassword?: string;
}

// Used for the add form
export type NewEmployee = Pick<Employee,
  'employeeNo' | 'nameEn' | 'nameAr' | 'username' | 'birthdate' |
  'nationalNo' | 'gender' | 'mobileNumber' | 'email' | 'startWorkingDate'
> & { departmentId: number | null };

export interface CreateEmployeeResponse
{
    message: string;
    employee: Employee;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/employees';

    getEmployees(): Observable<Employee[]>
    {
        return this.http.get<Employee[]>(this.apiUrl);
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
}