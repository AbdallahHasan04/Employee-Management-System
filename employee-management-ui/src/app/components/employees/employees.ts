import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService, Employee, NewEmployee } from '../../services/employee';
import { DepartmentService, Department } from '../../services/department';
import { NavbarComponent } from '../navbar/navbar';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-employees',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSelectModule, MatIconModule, NavbarComponent,
  ],
  templateUrl: './employees.html',
  styleUrl: './employees.css',
})
export class EmployeesComponent implements OnInit
{
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private cdr = inject(ChangeDetectorRef);

  employees: Employee[] = [];
  departments: Department[] = [];
  displayedColumns: string[] = [
    'employeeNo', 'nameEn', 'nameAr', 'departmentName', 'username', 'nationalNo',
    'gender', 'birthdate', 'mobileNumber', 'email', 'startWorkingDate',
    'status', 'actions'
  ];

  newEmployee: NewEmployee = this.emptyNewEmployee();
  editingEmployee: Employee | null = null;

  lastCreatedUsername: string | null = null;
  lastGeneratedPassword: string | null = null;

  ngOnInit()
  {
    this.loadData();
    this.loadDepartments();
  }

  private emptyNewEmployee(): NewEmployee
  {
    return {
      employeeNo: '', nameEn: '', nameAr: '', username: '',
      birthdate: null, nationalNo: '', gender: '',
      mobileNumber: null, email: null, startWorkingDate: null,
      departmentId: null
    };
  }

  loadData()
  {
    this.employeeService.getEmployees().subscribe({
      next: (data) => {
        this.employees = [...data];
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        alert('Could not fetch employee list. Verify your backend is running');
      }
    });
  }

  loadDepartments()
  {
    this.departmentService.getDepartments().subscribe({
      next: (data) => {
        this.departments = [...data];
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Could not fetch departments.', error);
      }
    });
  }

  onAdd(): void
  {
    const requiredStrings = [this.newEmployee.employeeNo, this.newEmployee.nameEn, this.newEmployee.username, this.newEmployee.nationalNo];
    if (requiredStrings.some(f => !f?.trim())) {
      alert('Employee No, Name (EN), Username and National No are required.');
      return;
    }

    if (!this.newEmployee.departmentId) {
      alert('Please select a department.');
      return;
    }

    this.employeeService.addEmployee(this.newEmployee).subscribe({
      next: (response) => {
        this.lastCreatedUsername = response.employee.username;
        this.lastGeneratedPassword = response.employee.generatedPassword ?? null;
        this.newEmployee = this.emptyNewEmployee();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
  }

  onEdit(employee: Employee)
  {
    this.editingEmployee = { ...employee };
  }

  onUpdate()
  {
    if (!this.editingEmployee) return;

    if (!this.editingEmployee.nameEn.trim() || !this.editingEmployee.nationalNo.trim() || !this.editingEmployee.departmentId) {
      alert('Name (EN), National No and Department are required.');
      return;
    }

    this.employeeService.updateEmployee(this.editingEmployee).subscribe({
      next: (response) => {
        console.log(response.message);
        this.editingEmployee = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Update failed.', error);
        if (error.status === 404) {
          alert(error.error?.message || 'Employee not found.');
        }
      }
    });
  }

  onDelete(id: number)
  {
    this.employeeService.deleteEmployee(id).subscribe({
      next: (response) => {
        console.log(response.message);
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Delete failed.', error);
        if (error.status === 404) {
          alert(error.error?.message || 'Employee not found.');
        }
      }
    });
  }

  dismissCredentials()
  {
    this.lastCreatedUsername = null;
    this.lastGeneratedPassword = null;
  }

  statusClass(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'pill-active' : 'pill-inactive';
  }

  toggleStatus(item: Employee): void
  {
    const newStatus = item.status === 'Active' ? 'Inactive' : 'Active';
    const updated: Employee = { ...item, status: newStatus };

    this.employeeService.updateEmployee(updated).subscribe({
      next: () => {
        this.loadData();
      },
      error: (error) => {
        console.error('API Error: Status toggle failed.', error);
        alert('Could not update status.');
      }
    });
  }
}