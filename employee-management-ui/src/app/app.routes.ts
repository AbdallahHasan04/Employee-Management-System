import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login' 
import { EmployeesComponent } from './components/employees/employees';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employees', component: EmployeesComponent },
];
