import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { first } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header bg-primary text-white">
              <h4 class="mb-0">Login</h4>
            </div>
            <div class="card-body">
              <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
                <div class="alert alert-danger" *ngIf="error">
                  {{error}}
                </div>
                
                <div class="form-group mb-3">
                  <label for="email">Email</label>
                  <input 
                    type="email" 
                    formControlName="email" 
                    class="form-control" 
                    [ngClass]="{ 'is-invalid': submitted && f['email'].errors }" />
                  <div *ngIf="submitted && f['email'].errors" class="invalid-feedback">
                    <div *ngIf="f['email'].errors['required']">Email is required</div>
                    <div *ngIf="f['email'].errors['email']">Enter a valid email address</div>
                  </div>
                </div>
                
                <div class="form-group mb-3">
                  <label for="password">Password</label>
                  <input 
                    type="password" 
                    formControlName="password" 
                    class="form-control" 
                    [ngClass]="{ 'is-invalid': submitted && f['password'].errors }" />
                  <div *ngIf="submitted && f['password'].errors" class="invalid-feedback">
                    <div *ngIf="f['password'].errors['required']">Password is required</div>
                  </div>
                </div>
                
                <div class="form-group">
                  <button [disabled]="loading" class="btn btn-primary">
                    <span *ngIf="loading" class="spinner-border spinner-border-sm mr-1"></span>
                    Login
                  </button>
                  <a routerLink="/register" class="btn btn-link">Register</a>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  returnUrl: string = '/';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) { 
    // Redirect to home if already logged in
    if (this.authService.isLoggedIn) {
      this.router.navigate(['/']);
    }
  }

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    // Get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  // Convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  onSubmit(): void {
    this.submitted = true;

    // Stop here if form is invalid
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.authService.login({
      email: this.f['email'].value,
      password: this.f['password'].value
    })
      .pipe(first())
      .subscribe({
        next: () => {
          this.router.navigate([this.returnUrl]);
        },
        error: error => {
          this.error = error.error?.message || 'Login failed. Please check your credentials.';
          this.loading = false;
        }
      });
  }
}
