import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/Users/user.service';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, RouterModule],
  templateUrl: './signin.html',
  styleUrls: ['./signin.css']
})
export class SigninComponent {
  loginForm: FormGroup;
  submitted = false;
  errorMessage: string | null = null;

  // Inject Router here
  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      emailOrMobile: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessage = null;

    if (this.loginForm.invalid) return;

    this.userService.login(this.loginForm.value).subscribe({
      next: (res: any) => {
        console.log('Login response:', res);  // Always log to debug
        if (res && res.token) {                // backend sends token only on success
          console.log('Login successful');
          localStorage.setItem('token', res.token);
          this.router.navigateByUrl('/');
        } else {
          this.errorMessage = res?.message || 'Invalid credentials';
        }
      }
      ,
      error: (err: any) => {
        console.error(err);
        this.errorMessage = 'Login failed. Please try again later.';
      }
    });
  }

  onForgotPassword() {
    console.log('Navigate to Forgot Password page');
  }

  onSignup() {
    this.router.navigate(['signup']);
  }
}
