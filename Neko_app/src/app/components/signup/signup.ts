import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/Users/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './signup.html',
  styleUrls: ['./signup.css']
})
export class SignupComponent {
  signupForm: FormGroup;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.signupForm = this.fb.group(
      {
        name: ['', [Validators.required, Validators.minLength(2)]],
        username: ['', [Validators.required, Validators.minLength(2), Validators.pattern('^[a-zA-Z0-9_]+$')]],
        mobile: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern('^(?=.*[A-Z])(?=.*[!@#$%^&*()_+\\-=[\\]{};:"\\\\|,.<>/?]).+$')
        ]],
        confirmPassword: ['', Validators.required]
      },
      { validators: this.passwordMatchValidator }
    );
  }

  // Password matching validator
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  get f(): { [key: string]: AbstractControl } {
    return this.signupForm.controls;
  }

  onSubmit() {
    console.log('Submit clicked', this.signupForm.value);

    this.errorMessage = '';
    this.successMessage = '';

    if (this.signupForm.invalid) {
      this.errorMessage = 'Please fix the errors in the form.';
      return;
    }

    const { username, email, mobile } = this.signupForm.value;

    // ✅ Correct method names
    this.userService.checkuser({ username, email, mobile }).subscribe({
      next: (res: { usernameExists: boolean; emailExists: boolean; mobileExists: boolean }) => {

        // Frontend validations
        if (res.emailExists) {
          this.errorMessage = 'User already has an account with this email.';
          return;
        }
        if (res.mobileExists) {
          this.errorMessage = 'User already has an account with this mobile.';
          return;
        }
        if (res.usernameExists) {
          this.errorMessage = 'Username is already taken.';
          return;
        }

        // Add user if all checks pass
        this.userService.signup(this.signupForm.value).subscribe({
          next: () => {
            this.successMessage = 'User registered successfully!';
            this.signupForm.reset();
            this.router.navigate(['/signin']);
          },
          error: (err: any) => {
            console.error('Error adding user:', err);
            this.errorMessage = err?.error || 'Error adding user. Try again.';
          }
        });
      },
      error: (err: any) => {
        console.error('Error checking user:', err);
        this.errorMessage = 'Error checking user. Try again.';
      }
    });
  }
}
