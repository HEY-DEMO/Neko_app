import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../services/Users/user.service';

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './signin.html',
  styleUrls: ['./signin.css']
})
export class SigninComponent {
  signinForm: FormGroup;
  submitted = false;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.signinForm = this.fb.group({
      emailOrMobile: ['', [
        Validators.required,
        Validators.pattern(/^(\d{10}|[a-zA-Z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4})$/)
      ]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  get f() {
    return this.signinForm.controls;
  }

  onSubmit() {
    this.submitted = true;

    if (this.signinForm.invalid) {
      return;
    }

    const { emailOrMobile, password } = this.signinForm.value;

    this.userService.login(emailOrMobile, password).subscribe({
      next: (res: any) => {
        console.log('Login successful', res);

        this.errorMessage = '';

        this.router.navigate(['/']);
      },
      error: (err: any) => {
        console.error('Login failed:', err);
        this.errorMessage = 'Invalid credentials. Please try again.';
      }
    });
  }


  goToSignup() {
    this.router.navigate(['/signup']);
  }

  forgotPassword() {
    this.router.navigate(['/forgot-password']);
  }
}
