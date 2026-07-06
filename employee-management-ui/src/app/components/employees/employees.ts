import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService, Employee } from '../../services/employee';

import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component
(
  {
  selector: 'app-employees',
  imports: 
  [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './employees.html',
  styleUrl: './employees.css',
  }
)
export class EmployeesComponent implements OnInit 
{
  private EmployeeService = inject(EmployeeService);

  employees: Employee[] = [];
  displayedColumns: string[] = ['id', 'name', 'email', 'position', 'actions'];
  newName: string = '';
  newEmail: string = '';
  newPosition: string = '';
  editingEmployee: Employee | null = null;

  ngOnInit() 
{
    this.loadData();
  }

  loadData() 
  {
    this.employees = [...this.EmployeeService.getEmployees()];
  }

  onAdd() : void 
  {
    if(this.newName && this.newEmail)
      {
      this.EmployeeService.addEmployee(this.newName, this.newPosition, this.newEmail);
      this.newName = '';
      this.newEmail = '';
      this.newPosition = '';
      this.loadData();
    }
  }

  onEdit(employee: Employee) 
  {
    this.editingEmployee = 
    {
      id: employee.id,
      name: employee.name,
      email: employee.email,
      position: employee.position
    };
  }

  onUpdate() 
  {
    if (this.editingEmployee) 
      {
      this.EmployeeService.updateEmployee(this.editingEmployee);
      this.editingEmployee = null;
      this.loadData();
    }
  }

  onDelete(id: number) 
  {
    this.EmployeeService.deleteEmployee(id);
    this.loadData();
  }
}
  

