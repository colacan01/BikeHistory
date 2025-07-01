import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Brand, Manufacturer } from '../../models/catalog.model';
import { CatalogService } from '../../services/catalog.service';

@Component({
  selector: 'app-brand-management',
  templateUrl: './brand-management.component.html',
  styleUrls: ['./brand-management.component.css']
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
