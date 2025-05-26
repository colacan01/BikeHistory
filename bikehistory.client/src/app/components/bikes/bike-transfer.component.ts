import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-transfer',
  template: `
    <div class="container mt-4">
      <h2>Transfer Bike Ownership</h2>
      
      <div class="alert alert-info" *ngIf="loading">
        Loading bike details...
      </div>

      <div class="alert alert-danger" *ngIf="error">
        {{error}}
      </div>

      <div *ngIf="bike && !loading">
        <div class="card mb-4">
          <div class="card-header">
            <h5 class="card-title mb-0">Bike Information</h5>
          </div>
          <div class="card-body">
            <p><strong>Frame Number:</strong> {{bike.frameNumber}}</p>
            <p><strong>Type:</strong> {{bike.bikeTypeName || 'Unknown Type'}}</p>
            <p><strong>Brand:</strong> {{bike.brandName || 'Unknown Brand'}}</p>
            <p *ngIf="bike.model"><strong>Model:</strong> {{bike.model}}</p>
            <p *ngIf="bike.manufactureYear"><strong>Year:</strong> {{bike.manufactureYear}}</p>
          </div>
        </div>

        <form [formGroup]="transferForm" (ngSubmit)="onSubmit()" class="mt-4">
          <div class="mb-3">
            <label for="newOwnerId" class="form-label">New Owner Email *</label>
            <input 
              type="email" 
              formControlName="newOwnerId" 
              class="form-control" 
              [ngClass]="{ 'is-invalid': submitted && f['newOwnerId'].errors }" />
            <div *ngIf="submitted && f['newOwnerId'].errors" class="invalid-feedback">
              <div *ngIf="f['newOwnerId'].errors['required']">New Owner Email is required</div>
              <div *ngIf="f['newOwnerId'].errors['email']">Please enter a valid email address</div>
            </div>
          </div>

          <div class="mb-3">
            <label for="notes" class="form-label">Transfer Notes</label>
            <textarea 
              formControlName="notes" 
              class="form-control" 
              rows="3"></textarea>
          </div>

          <div class="alert alert-warning">
            <strong>Warning:</strong> This action will transfer ownership of your bike to another user.
            Once transferred, you will no longer have access to manage this bike.
          </div>

          <div class="d-flex justify-content-between mt-4">
            <button type="button" class="btn btn-outline-secondary" [routerLink]="['/bikes', bikeId]">Cancel</button>
            <button type="submit" class="btn btn-primary" [disabled]="transferring">
              <span *ngIf="transferring" class="spinner-border spinner-border-sm me-1"></span>
              Transfer Ownership
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: []
})
export class BikeTransferComponent implements OnInit {
  bikeId!: number;
  bike: BikeFrame | null = null;
  transferForm!: FormGroup;
  loading = false;
  transferring = false;
  submitted = false;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private bikeService: BikeService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.bikeId = +params['id'];
      this.loadBikeDetails();
    });

    this.transferForm = this.formBuilder.group({
      newOwnerId: ['', [Validators.required, Validators.email]],
      notes: ['']
    });
  }

  // Convenience getter for easy access to form fields
  get f() { return this.transferForm.controls; }

  loadBikeDetails(): void {
    this.loading = true;
    this.bikeService.getBikeFrameById(this.bikeId)
      .subscribe({
        next: bike => {
          this.bike = bike;
          this.loading = false;
        },
        error: error => {
          this.error = 'Failed to load bike details. Please try again later.';
          this.loading = false;
          console.error('Error loading bike details:', error);
        }
      });
  }

  onSubmit(): void {
    this.submitted = true;

    // Stop here if form is invalid
    if (this.transferForm.invalid) {
      return;
    }

    this.transferring = true;
    this.bikeService.transferOwnership(this.bikeId, {
      newOwnerId: this.f['newOwnerId'].value,
      notes: this.f['notes'].value
    })
      .subscribe({
        next: () => {
          this.router.navigate(['/bikes'], { 
            queryParams: { transferred: true } 
          });
        },
        error: error => {
          this.error = error.error?.message || 'Failed to transfer ownership. Please try again.';
          this.transferring = false;
        }
      });
  }
}
