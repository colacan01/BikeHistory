import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BikeType } from '../../models/catalog.model';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-bike-type-management',
  templateUrl: './bike-type-management.component.html',
  styles: []
})
export class BikeTypeManagementComponent implements OnInit {
  bikeTypeForm!: FormGroup;
  bikeTypes: BikeType[] = [];
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
    this.loadBikeTypes();
  }

  get f() { return this.bikeTypeForm.controls; }

  initForm(): void {
    this.bikeTypeForm = this.formBuilder.group({
      name: ['', Validators.required],
      description: ['']
    });
  }

  loadBikeTypes(): void {
    this.loadingList = true;
    this.listError = '';

    this.catalogService.getBikeTypes().subscribe({
      next: (data) => {
        this.bikeTypes = data;
        this.loadingList = false;
      },
      error: (error) => {
        this.listError = 'Failed to load bike types. Please try again.';
        this.loadingList = false;
        console.error('Error loading bike types:', error);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.bikeTypeForm.invalid) {
      return;
    }

    this.loading = true;

    const bikeTypeData: BikeType = {
      id: this.isEditing ? this.currentId! : 0,
      name: this.f['name'].value,
      description: this.f['description'].value
    };

    if (this.isEditing) {
      this.updateBikeType(bikeTypeData);
    } else {
      this.addBikeType(bikeTypeData);
    }
  }

  addBikeType(bikeType: BikeType): void {
    this.catalogService.createBikeType(bikeType).subscribe({
      next: () => {
        this.success = 'Bike type added successfully.';
        this.loading = false;
        this.resetForm();
        this.loadBikeTypes();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to add bike type. Please try again.';
        this.loading = false;
        console.error('Error adding bike type:', error);
      }
    });
  }

  updateBikeType(bikeType: BikeType): void {
    this.catalogService.updateBikeType(bikeType.id, bikeType).subscribe({
      next: () => {
        this.success = 'Bike type updated successfully.';
        this.loading = false;
        this.resetForm();
        this.loadBikeTypes();
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to update bike type. Please try again.';
        this.loading = false;
        console.error('Error updating bike type:', error);
      }
    });
  }

  editBikeType(bikeType: BikeType): void {
    this.isEditing = true;
    this.currentId = bikeType.id;
    this.bikeTypeForm.patchValue({
      name: bikeType.name,
      description: bikeType.description
    });
  }

  deleteBikeType(id: number): void {
    if (confirm('Are you sure you want to delete this bike type?')) {
      this.catalogService.deleteBikeType(id).subscribe({
        next: () => {
          this.success = 'Bike type deleted successfully.';
          this.loadBikeTypes();
        },
        error: (error) => {
          this.error = error.error?.message || 'Failed to delete bike type. Please try again.';
          console.error('Error deleting bike type:', error);
        }
      });
    }
  }

  resetForm(): void {
    this.submitted = false;
    this.isEditing = false;
    this.currentId = undefined;
    this.bikeTypeForm.reset();
  }
}
