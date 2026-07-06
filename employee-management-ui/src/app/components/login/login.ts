import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';

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
  //injecting needed items
  private formbuilder = inject(FormBuilder);
  private router = inject(Router);

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

      //temp validation, will replace later
      if( username == 'admin' && password == 'admin')
      {
        this.router.navigate(['/employees']);
      }
      else
      {
        this.errormessage = 'Invalid Username or Password';
      }
    }
  }
}
