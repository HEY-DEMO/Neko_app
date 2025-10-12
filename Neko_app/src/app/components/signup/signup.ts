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

  constructor(private fb: FormBuilder, private userService: UserService, private router: Router) {
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


  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  get f(): { [key: string]: AbstractControl } {
    return this.signupForm.controls;
  }

  onSubmit() {
    console.log('Submit clicked', this.signupForm.value); // DEBUG: Ensure submit is triggered
    this.errorMessage = '';
    this.successMessage = '';

    // Check form validity
    if (this.signupForm.invalid) {
      this.errorMessage = 'Please fix the errors in the form.';
      console.log('Form invalid', this.signupForm.errors);
      return;
    }

    const { username, email, mobile } = this.signupForm.value;

    // Step 1: Check if user exists
    this.userService.checkuser({ username, email, mobile }).subscribe({
      next: (res: { usernameExists: boolean; emailExists: boolean; mobileExists: boolean }) => {

        // Step 1: Show frontend validation messages
        if (res.emailExists) {
          alert('User already has an account with this email.');
          return;
        }
        if (res.mobileExists) {
          alert('User already has an account with this mobile.');
          return;
        }
        if (res.usernameExists) {
          alert('Username is already taken.');
          return;
        }

        // Step 2: Add user if all checks pass
        this.userService.adduser(this.signupForm.value).subscribe({
          next: () => {
            this.successMessage = 'User registered successfully!';
            this.errorMessage = '';
            this.signupForm.reset();
            alert('User registered successfully!');
            this.router.navigate(['/signin']);
          },
          error: (err: any) => {
            console.error('Error adding user:', err);

            // Show backend error message if available
            if (err.error && typeof err.error === 'string') {
              this.errorMessage = err.error;
              alert(err.error); // Show backend validation error in popup
            } else {
              this.errorMessage = 'Error adding user. Try again.';
              alert('Error adding user. Try again.');
            }
          }
        });
      },
      error: (err: any) => {
        console.error('Error checking user:', err);
        this.errorMessage = 'Error checking user. Try again.';
        alert('Error checking user. Try again.');
      }
    });

  }
}
