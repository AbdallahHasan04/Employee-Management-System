import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
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
  private cdr = inject(ChangeDetectorRef);

  employees: Employee[] = [];
  displayedColumns: string[] = ['id', 'name', 'email', 'position', 'actions'];
  newName: string = '';
  newEmail: string = '';
  newPosition: string = '';
  editingEmployee: Employee | null = null;

  ngOnInit() 
  {
    console.log('ngOnInit fired');
    this.loadData();
  }

  loadData() 
  {
    console.log('loadData called, about to subscribe');
    this.EmployeeService.getEmployees().subscribe
    ({
      next: (data) => 
      {
        console.log('loadData got data:', data);
        this.employees = [...data];
        this.cdr.detectChanges();
      },
      error: (error) => 
      {
        console.error('API Error: Connection dropped.', error);
        alert('Could not fetch employee list. Verify your backend is running');
      }
    })
  }

  onAdd() : void 
  {
    if(!this.newName.trim() || !this.newEmail.trim() || !this.newPosition.trim())
      {
        alert('All fields are required.');
        return;
      }

    this.EmployeeService.addEmployee(this.newName, this.newPosition, this.newEmail).subscribe
    ({
      next: response => 
      {
        console.log(response.message);
        this.newName = '';
        this.newEmail = '';
        this.newPosition = '';
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
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
      if(!this.editingEmployee.name.trim() || !this.editingEmployee.email.trim() || !this.editingEmployee.position.trim())
        {
          alert('All fields are required.');
          return;
        }

      this.EmployeeService.updateEmployee(this.editingEmployee).subscribe
      ({
        next: (response) => 
        {
          console.log(response.message);
          this.editingEmployee = null;
          this.loadData();
          this.cdr.detectChanges();
        },
        error: (error) => 
        {
          console.error('API Error: Update failed.', error);
          if(error.status === 404)
          {
            alert(error.error?.message || 'Employee not found.');
          }
        }
      });
    }  
  }

  onDelete(id: number) 
  {
    this.EmployeeService.deleteEmployee(id).subscribe
    ({
      next: (response) => 
      {
        console.log(response.message);
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => 
      {
        console.error('API Error: Delete failed.', error);
        if(error.status === 404)
        {
          alert(error.error?.message || 'Employee not found.');
        }
      }
    });
  }
}
  

