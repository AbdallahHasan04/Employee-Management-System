import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DepartmentService, Department, NewDepartment } from '../../services/department';
import { AuthService } from '../../services/auth';
import { NavbarComponent } from '../navbar/navbar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-departments',
  imports: [
    CommonModule, FormsModule, MatTableModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, NavbarComponent, MatSelectModule,
  ],
  templateUrl: './departments.html',
  styleUrl: './departments.css',
})
export class DepartmentsComponent implements OnInit
{
  private departmentService = inject(DepartmentService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  departments: Department[] = [];
  displayedColumns: string[] = ['departmentCode', 'nameEn', 'nameAr', 'description', 'status', 'actions'];

  newDepartment: NewDepartment = this.emptyNewDepartment();
  editingDepartment: Department | null = null;

  ngOnInit()
  {
    this.loadData();
  }

  private emptyNewDepartment(): NewDepartment
  {
    return { departmentCode: '', nameEn: '', nameAr: '', description: null };
  }

  loadData()
  {
    this.departmentService.getDepartments().subscribe({
      next: (data) => {
        this.departments = [...data];
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Connection dropped.', error);
        alert('Could not fetch department list. Verify your backend is running');
      }
    });
  }

  onAdd(): void
  {
    const required = [this.newDepartment.departmentCode, this.newDepartment.nameEn, this.newDepartment.nameAr];
    if (required.some(f => !f?.trim())) {
      alert('Department Code, Name (EN) and Name (AR) are required.');
      return;
    }

    this.departmentService.addDepartment(this.newDepartment).subscribe({
      next: () => {
        this.newDepartment = this.emptyNewDepartment();
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => console.error('API Error: Add failed.', error)
    });
  }

  onEdit(department: Department)
  {
    this.editingDepartment = { ...department };
  }

  onUpdate()
  {
    if (!this.editingDepartment) return;

    if (!this.editingDepartment.nameEn.trim() || !this.editingDepartment.nameAr.trim()) {
      alert('Name (EN) and Name (AR) are required.');
      return;
    }

    this.departmentService.updateDepartment(this.editingDepartment).subscribe({
      next: (response) => {
        console.log(response.message);
        this.editingDepartment = null;
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Update failed.', error);
        if (error.status === 404) {
          alert(error.error?.message || 'Department not found.');
        }
      }
    });
  }

  onDelete(id: number)
  {
    this.departmentService.deleteDepartment(id).subscribe({
      next: (response) => {
        console.log(response.message);
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API Error: Delete failed.', error);
        alert(error.error?.message || 'Could not delete department.');
      }
    });
  }

  statusClass(status: string): string
  {
    return status?.toLowerCase() === 'active' ? 'pill-active' : 'pill-inactive';
  }

  toggleStatus(item: Department): void
  {
    const newStatus = item.status === 'Active' ? 'Inactive' : 'Active';
    const updated: Department = { ...item, status: newStatus };

    this.departmentService.updateDepartment(updated).subscribe({
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