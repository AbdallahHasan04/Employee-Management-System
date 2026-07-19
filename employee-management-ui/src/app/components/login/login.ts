import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/auth';

@Component
(
  {
  selector: 'app-login',
  imports: 
  [
  CommonModule, 
  ReactiveFormsModule,
  MatFormFieldModule, 
  MatInputModule, 
  MatButtonModule   
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
  }
)

export class LoginComponent 
{
  private formbuilder = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);

  loginform : FormGroup;
  errormessage = '';

  constructor()
  {
    this.loginform = this.formbuilder.group
    (
      {
      username: ['', Validators.required],
      password: ['', Validators.required]
      }
    );
  }

  onSubmit()
  {
    if(this.loginform.valid){
      const { username, password } = this.loginform.value;
      
      this.authService.login({ username, password }).subscribe({
        next: (response) => {
          this.errormessage = '';
          this.router.navigate(['/employees']);
        },
        error: (err) => {
          console.error('Login error details:', err);
          this.errormessage = 'Invalid Username or Password';
        }
      });
    }
  }
}