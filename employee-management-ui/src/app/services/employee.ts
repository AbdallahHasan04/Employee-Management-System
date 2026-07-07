import { Service } from '@angular/core';
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Employee 
{
    id: number;
    name: string;
    position: string;
    email: string;
}

@Injectable
(
    {
    providedIn: 'root'
    }
)

export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7038/api/employees';

    getEmployees(): Observable<Employee[]> 
    {
        return this.http.get<Employee[]>(this.apiUrl);
    }

    addEmployee(name: string, position: string, email: string): Observable<any> 
    {
        return this.http.post(this.apiUrl, { name, position, email });
    }

    updateEmployee(updatedEmployee: Employee): Observable<any> 
    {
        return this.http.put(this.apiUrl, updatedEmployee);    
    }

    deleteEmployee(id: number): Observable<any> 
    {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
