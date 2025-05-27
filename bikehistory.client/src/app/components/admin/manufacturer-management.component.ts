import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Manufacturer } from '../../models/catalog.model';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-manufacturer-management',
  templateUrl: './manufacturer-management.component.html',
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
