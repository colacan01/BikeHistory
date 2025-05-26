import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Manufacturer } from '../../models/catalog.model';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-manufacturer-management',
  template: `
    <div class="container mt-4">
      <h2>Manufacturer Management</h2>
      
      <!-- Form for adding/editing manufacturer -->
      <div class="card mb-4">
        <div class="card-header">
          <h4>{{ isEditing ? 'Edit Manufacturer' : 'Add Manufacturer' }}</h4>
        </div>
        <div class="card-body">
          <form [formGroup]="manufacturerForm" (ngSubmit)="onSubmit()">
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
              <label for="countryOfOrigin" class="form-label">Country of Origin</label>
              <input type="text" class="form-control" id="countryOfOrigin" formControlName="countryOfOrigin">
            </div>

            <div class="mb-3">
              <label for="website" class="form-label">Website</label>
              <input type="url" class="form-control" id="website" formControlName="website">
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
                {{ isEditing ? 'Update' : 'Add' }} Manufacturer
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Manufacturers list -->
      <div class="card">
        <div class="card-header">
          <h4>Manufacturers</h4>
        </div>
        <div class="card-body">
          <div class="alert alert-info" *ngIf="loadingList">Loading manufacturers...</div>
          <div class="alert alert-danger" *ngIf="listError">{{ listError }}</div>

          <div class="table-responsive" *ngIf="!loadingList && !listError">
            <table class="table table-striped">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Country</th>
                  <th>Website</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngIf="manufacturers.length === 0">
                  <td colspan="4" class="text-center">No manufacturers found</td>
                </tr>
                <tr *ngFor="let manufacturer of manufacturers">
                  <td>{{ manufacturer.name }}</td>
                  <td>{{ manufacturer.countryOfOrigin || '-' }}</td>
                  <td>
                    <a *ngIf="manufacturer.website" [href]="manufacturer.website" target="_blank">
                      {{ manufacturer.website }}
                    </a>
                    <span *ngIf="!manufacturer.website">-</span>
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary me-2" (click)="editManufacturer(manufacturer)">
                      Edit
                    </button>
                    <button class="btn btn-sm btn-outline-danger" (click)="deleteManufacturer(manufacturer.id)">
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
export class ManufacturerManagementComponent implements OnInit {
  manufacturerForm!: FormGroup;
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
  }

  get f() { return this.manufacturerForm.controls; }

  initForm(): void {
    this.manufacturerForm = this.formBuilder.group({
      name: ['', Validators.required],
      countryOfOrigin: [''],
      website: [''],
      description: ['']
    });
  }

  loadManufacturers(): void {
    this.loadingList = true;
    this.listError = '';

    this.catalogService.getManufacturers().subscribe({
      next: (data) => {
        this.manufacturers = data;
        this.loadingList = false;
      },
      error: (error) => {
        this.listError = 'Failed to load manufacturers. Please try again.';
        this.loadingList = false;
        console.error('Error loading manufacturers:', error);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.manufacturerForm.invalid) {
      return;
    }

    this.loading = true;

    const manufacturerData: Manufacturer = {
      id: this.isEditing ? this.currentId! : 0,
      name: this.f['name'].value,
      countryOfOrigin: this.f['countryOfOrigin'].value,
      website: this.f['website'].value,
      description: this.f['description'].value
    };

    if (this.isEditing) {
      this.updateManufacturer(manufacturerData);
    } else {
      this.addManufacturer(manufacturerData);
    }
  }

  addManufacturer(manufacturer: Manufacturer): void {
    this.catalogService.createManufacturer(manufacturer).subscribe({
      next: () => {
        this.success = 'Manufacturer added successfully.';
        this.loading = false;
        this.resetForm();
        this.loadManufacturers();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to add manufacturer. Please try again.';
        this.loading = false;
        console.error('Error adding manufacturer:', error);
      }
    });
  }

  updateManufacturer(manufacturer: Manufacturer): void {
    this.catalogService.updateManufacturer(manufacturer.id, manufacturer).subscribe({
      next: () => {
        this.success = 'Manufacturer updated successfully.';
        this.loading = false;
        this.resetForm();
        this.loadManufacturers();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update manufacturer. Please try again.';
        this.loading = false;
        console.error('Error updating manufacturer:', error);
      }
    });
  }

  editManufacturer(manufacturer: Manufacturer): void {
    this.isEditing = true;
    this.currentId = manufacturer.id;
    this.manufacturerForm.patchValue({
      name: manufacturer.name,
      countryOfOrigin: manufacturer.countryOfOrigin,
      website: manufacturer.website,
      description: manufacturer.description
    });
  }

  deleteManufacturer(id: number): void {
    if (confirm('Are you sure you want to delete this manufacturer?')) {
      this.catalogService.deleteManufacturer(id).subscribe({
        next: () => {
          this.success = 'Manufacturer deleted successfully.';
          this.loadManufacturers();
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to delete manufacturer. Please try again.';
          console.error('Error deleting manufacturer:', error);
        }
      });
    }
  }

  resetForm(): void {
    this.submitted = false;
    this.isEditing = false;
    this.currentId = undefined;
    this.manufacturerForm.reset();
  }
}
