import { Service } from '@angular/core';
import { Injectable } from '@angular/core';


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
    private employees: Employee[] = 
    [
        {
            id: 1,
            name: 'Abdallah',
            position: 'Software Engineer',
            email: 'abdallah@example.com'
        },
        {
            id: 2,
            name: 'Ali',
            position: 'Product Manager',
            email: 'ali@example.com'
        }
    ];

    private nextId = 3;

    getEmployees(): Employee[] 
    {
        return this.employees;
    }

    addEmployee( name: string, position: string, email: string): void 
    {
        const newEmployee: Employee = 
        {
            id: this.nextId++,
            name: name,
            position: position,
            email: email
        };
        this.employees.push(newEmployee);
    }

    updateEmployee(updatedEmployee: Employee): void 
    {
        const index = this.employees.findIndex(emp => emp.id === updatedEmployee.id);
        if (index !== -1) 
        {
            this.employees[index] = updatedEmployee;
        }
    }

    deleteEmployee(id: number): void 
    {
        this.employees = this.employees.filter(emp => emp.id !== id);
    }
}
