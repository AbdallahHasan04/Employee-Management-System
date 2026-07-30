import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login';
import { EmployeesComponent } from './components/employees/employees';
import { DepartmentsComponent } from './components/departments/departments';
import { PositionsComponent } from './components/positions/positions';
import { PositionHistoryComponent } from './components/position-history/position-history';
import { NotFoundComponent } from './components/not-found/not-found';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'employees', component: EmployeesComponent, canActivate: [authGuard] },
  { path: 'departments', component: DepartmentsComponent, canActivate: [authGuard] },
  { path: 'positions', component: PositionsComponent, canActivate: [authGuard] },
  { path: 'position-history', component: PositionHistoryComponent, canActivate: [authGuard] },
  { path: 'not-found', component: NotFoundComponent },
  { path: '**', component: NotFoundComponent },
];