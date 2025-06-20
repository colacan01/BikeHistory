import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BikeType, Brand, Manufacturer } from '../../models/catalog.model';
import { BikeService } from '../../services/bike.service';
import { CatalogService } from '../../services/catalog.service';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-bike-register',
  templateUrl: './bike-register.component.html',
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
    private catalogService: CatalogService,
    private activityLogger: ActivityLoggerService
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
          // 자전거 프레임 등록 로깅
          this.activityLogger.logAction('RegistBikeFrame', {
            frameNumber: this.f['frameNumber'].value,
            manufacturerId: this.f['manufacturerId'].value,
            brandId: this.f['brandId'].value,
            bikeTypeId: this.f['bikeTypeId'].value,
            model: this.f['model'].value,
            manufactureYear: this.f['manufactureYear'].value,
            color: this.f['color'].value
          });

          this.router.navigate(['/bikes']);
        },
        error: error => {
          this.error = error.error?.message || 'Failed to register bike. Please try again.';
          this.loading = false;
        }
      });
  }
}
