import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BikeType, Brand, Manufacturer } from '../../models/catalog.model';
import { BikeService } from '../../services/bike.service';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-bike-register',
  template: `
    <div class="container mt-4">
      <h2>Register New Bike</h2>
      
      <div class="alert alert-danger" *ngIf="error">
        {{error}}
      </div>

      <form [formGroup]="bikeForm" (ngSubmit)="onSubmit()" class="mt-4">
        <div class="row">
          <div class="col-md-6 mb-3">
            <label for="frameNumber" class="form-label">Frame Number *</label>
            <input 
              type="text" 
              formControlName="frameNumber" 
              class="form-control" 
              [ngClass]="{ 'is-invalid': submitted && f['frameNumber'].errors }" />
            <div *ngIf="submitted && f['frameNumber'].errors" class="invalid-feedback">
              <div *ngIf="f['frameNumber'].errors['required']">Frame Number is required</div>
            </div>
          </div>

          <div class="col-md-6 mb-3">
            <label for="bikeTypeId" class="form-label">Bike Type *</label>
            <select 
              formControlName="bikeTypeId" 
              class="form-select" 
              [ngClass]="{ 'is-invalid': submitted && f['bikeTypeId'].errors }">
              <option value="">-- Select Bike Type --</option>
              <option *ngFor="let type of bikeTypes" [value]="type.id">{{type.name}}</option>
            </select>
            <div *ngIf="submitted && f['bikeTypeId'].errors" class="invalid-feedback">
              <div *ngIf="f['bikeTypeId'].errors['required']">Bike Type is required</div>
            </div>
          </div>
        </div>

        <div class="row">
          <div class="col-md-6 mb-3">
            <label for="manufacturerId" class="form-label">Manufacturer *</label>
            <select 
              formControlName="manufacturerId" 
              class="form-select" 
              [ngClass]="{ 'is-invalid': submitted && f['manufacturerId'].errors }"
              (change)="onManufacturerChange()">
              <option value="">-- Select Manufacturer --</option>
              <option *ngFor="let manufacturer of manufacturers" [value]="manufacturer.id">{{manufacturer.name}}</option>
            </select>
            <div *ngIf="submitted && f['manufacturerId'].errors" class="invalid-feedback">
              <div *ngIf="f['manufacturerId'].errors['required']">Manufacturer is required</div>
            </div>
          </div>

          <div class="col-md-6 mb-3">
            <label for="brandId" class="form-label">Brand *</label>
            <select 
              formControlName="brandId" 
              class="form-select" 
              [ngClass]="{ 'is-invalid': submitted && f['brandId'].errors }">
              <option value="">-- Select Brand --</option>
              <option *ngFor="let brand of brands" [value]="brand.id">{{brand.name}}</option>
            </select>
            <div *ngIf="submitted && f['brandId'].errors" class="invalid-feedback">
              <div *ngIf="f['brandId'].errors['required']">Brand is required</div>
            </div>
          </div>
        </div>

        <div class="row">
          <div class="col-md-4 mb-3">
            <label for="model" class="form-label">Model</label>
            <input type="text" formControlName="model" class="form-control" />
          </div>

          <div class="col-md-4 mb-3">
            <label for="manufactureYear" class="form-label">Year</label>
            <input 
              type="number" 
              formControlName="manufactureYear" 
              class="form-control"
              min="1900" 
              [max]="currentYear" />
          </div>

          <div class="col-md-4 mb-3">
            <label for="color" class="form-label">Color</label>
            <input type="text" formControlName="color" class="form-control" />
          </div>
        </div>

        <div class="d-flex justify-content-between mt-4">
          <button type="button" class="btn btn-outline-secondary" routerLink="/bikes">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-1"></span>
            Register Bike
          </button>
        </div>
      </form>
    </div>
  `,
  styles: []
})
export class BikeRegisterComponent implements OnInit {
  bikeForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  
  manufacturers: Manufacturer[] = [];
  brands: Brand[] = [];
  bikeTypes: BikeType[] = [];
  
  currentYear = new Date().getFullYear();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private bikeService: BikeService,
    private catalogService: CatalogService
  ) { }

  ngOnInit(): void {
    this.bikeForm = this.formBuilder.group({
      frameNumber: ['', Validators.required],
      manufacturerId: ['', Validators.required],
      brandId: ['', Validators.required],
      bikeTypeId: ['', Validators.required],
      model: [''],
      manufactureYear: [null],
      color: ['']
    });

    this.loadCatalogData();
  }

  // Convenience getter for easy access to form fields
  get f() { return this.bikeForm.controls; }

  loadCatalogData(): void {
    // Load manufacturers
    this.catalogService.getManufacturers().subscribe({
      next: data => this.manufacturers = data,
      error: error => console.error('Error loading manufacturers:', error)
    });

    // Load all brands initially
    this.catalogService.getBrands().subscribe({
      next: data => this.brands = data,
      error: error => console.error('Error loading brands:', error)
    });

    // Load bike types
    this.catalogService.getBikeTypes().subscribe({
      next: data => this.bikeTypes = data,
      error: error => console.error('Error loading bike types:', error)
    });
  }

  onManufacturerChange(): void {
    const manufacturerId = this.f['manufacturerId'].value;
    if (manufacturerId) {
      this.catalogService.getBrandsByManufacturer(manufacturerId).subscribe({
        next: data => this.brands = data,
        error: error => console.error('Error loading brands for manufacturer:', error)
      });
    } else {
      // If no manufacturer is selected, load all brands
      this.catalogService.getBrands().subscribe({
        next: data => this.brands = data,
        error: error => console.error('Error loading brands:', error)
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;

    // Stop here if form is invalid
    if (this.bikeForm.invalid) {
      return;
    }

    this.loading = true;
    this.bikeService.registerBikeFrame({
      frameNumber: this.f['frameNumber'].value,
      manufacturerId: this.f['manufacturerId'].value,
      brandId: this.f['brandId'].value,
      bikeTypeId: this.f['bikeTypeId'].value,
      model: this.f['model'].value,
      manufactureYear: this.f['manufactureYear'].value,
      color: this.f['color'].value
    })
      .subscribe({
        next: () => {
          this.router.navigate(['/bikes']);
        },
        error: error => {
          this.error = error.error?.message || 'Failed to register bike. Please try again.';
          this.loading = false;
        }
      });
  }
}
