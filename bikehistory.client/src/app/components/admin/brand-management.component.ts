import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Brand, Manufacturer } from '../../models/catalog.model';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-brand-management',
  template: `
    <div class="container mt-4">
      <h2>Brand Management</h2>
      
      <!-- Form for adding/editing brand -->
      <div class="card mb-4">
        <div class="card-header">
          <h4>{{ isEditing ? 'Edit Brand' : 'Add Brand' }}</h4>
        </div>
        <div class="card-body">
          <form [formGroup]="brandForm" (ngSubmit)="onSubmit()">
            <div class="alert alert-danger" *ngIf="error">{{ error }}</div>
            <div class="alert alert-success" *ngIf="success">{{ success }}</div>

            <div class="mb-3">
              <label for="name" class="form-label">Name *</label>
              <input type="text" class="form-control" id="name" formControlName="name"
                [ngClass]="{ 'is-invalid': submitted && f['name'].errors }">
              <div *ngIf="submitted && f['name'].errors" class="invalid-feedback">
                <div *ngIf="f['name'].errors['required']">Name is required</div>
              </div>
            </div>

            <div class="mb-3">
              <label for="manufacturerId" class="form-label">Manufacturer</label>
              <select class="form-select" id="manufacturerId" formControlName="manufacturerId">
                <option value="">-- Select Manufacturer --</option>
                <option *ngFor="let manufacturer of manufacturers" [value]="manufacturer.id">
                  {{ manufacturer.name }}
                </option>
              </select>
            </div>

            <div class="mb-3">
              <label for="description" class="form-label">Description</label>
              <textarea class="form-control" id="description" formControlName="description" rows="3"></textarea>
            </div>

            <div class="d-flex justify-content-between">
              <button type="button" class="btn btn-secondary" (click)="resetForm()">
                {{ isEditing ? 'Cancel' : 'Clear' }}
              </button>
              <button type="submit" class="btn btn-primary" [disabled]="loading">
                <span *ngIf="loading" class="spinner-border spinner-border-sm me-1"></span>
                {{ isEditing ? 'Update' : 'Add' }} Brand
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Brands list -->
      <div class="card">
        <div class="card-header">
          <h4>Brands</h4>
        </div>
        <div class="card-body">
          <div class="alert alert-info" *ngIf="loadingList">Loading brands...</div>
          <div class="alert alert-danger" *ngIf="listError">{{ listError }}</div>

          <div class="table-responsive" *ngIf="!loadingList && !listError">
            <table class="table table-striped">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Manufacturer</th>
                  <th>Description</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngIf="brands.length === 0">
                  <td colspan="4" class="text-center">No brands found</td>
                </tr>
                <tr *ngFor="let brand of brands">
                  <td>{{ brand.name }}</td>
                  <td>{{ brand.manufacturerName || 'None' }}</td>
                  <td>{{ brand.description || '-' }}</td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary me-2" (click)="editBrand(brand)">
                      Edit
                    </button>
                    <button class="btn btn-sm btn-outline-danger" (click)="deleteBrand(brand.id)">
                      Delete
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class BrandManagementComponent implements OnInit {
  brandForm!: FormGroup;
  brands: Brand[] = [];
  manufacturers: Manufacturer[] = [];
  isEditing = false;
  currentId?: number;
  submitted = false;
  loading = false;
  loadingList = false;
  error = '';
  success = '';
  listError = '';

  constructor(
    private formBuilder: FormBuilder,
    private catalogService: CatalogService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadManufacturers();
    this.loadBrands();
  }

  get f() { return this.brandForm.controls; }

  initForm(): void {
    this.brandForm = this.formBuilder.group({
      name: ['', Validators.required],
      manufacturerId: [''],
      description: ['']
    });
  }

  loadManufacturers(): void {
    this.catalogService.getManufacturers().subscribe({
      next: (data) => {
        this.manufacturers = data;
      },
      error: (error) => {
        console.error('Error loading manufacturers:', error);
      }
    });
  }

  loadBrands(): void {
    this.loadingList = true;
    this.listError = '';

    this.catalogService.getBrands().subscribe({
      next: (data) => {
        this.brands = data;
        this.loadingList = false;
      },
      error: (error) => {
        this.listError = 'Failed to load brands. Please try again.';
        this.loadingList = false;
        console.error('Error loading brands:', error);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.brandForm.invalid) {
      return;
    }

    this.loading = true;

    const brandData: Brand = {
      id: this.isEditing ? this.currentId! : 0,
      name: this.f['name'].value,
      manufacturerId: this.f['manufacturerId'].value ? Number(this.f['manufacturerId'].value) : undefined,
      description: this.f['description'].value
    };

    if (this.isEditing) {
      this.updateBrand(brandData);
    } else {
      this.addBrand(brandData);
    }
  }

  addBrand(brand: Brand): void {
    this.catalogService.createBrand(brand).subscribe({
      next: () => {
        this.success = 'Brand added successfully.';
        this.loading = false;
        this.resetForm();
        this.loadBrands();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to add brand. Please try again.';
        this.loading = false;
        console.error('Error adding brand:', error);
      }
    });
  }

  updateBrand(brand: Brand): void {
    this.catalogService.updateBrand(brand.id, brand).subscribe({
      next: () => {
        this.success = 'Brand updated successfully.';
        this.loading = false;
        this.resetForm();
        this.loadBrands();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update brand. Please try again.';
        this.loading = false;
        console.error('Error updating brand:', error);
      }
    });
  }

  editBrand(brand: Brand): void {
    this.isEditing = true;
    this.currentId = brand.id;
    this.brandForm.patchValue({
      name: brand.name,
      manufacturerId: brand.manufacturerId,
      description: brand.description
    });
  }

  deleteBrand(id: number): void {
    if (confirm('Are you sure you want to delete this brand?')) {
      this.catalogService.deleteBrand(id).subscribe({
        next: () => {
          this.success = 'Brand deleted successfully.';
          this.loadBrands();
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to delete brand. Please try again.';
          console.error('Error deleting brand:', error);
        }
      });
    }
  }

  resetForm(): void {
    this.submitted = false;
    this.isEditing = false;
    this.currentId = undefined;
    this.brandForm.reset();
  }
}
