import { Component, OnInit } from '@angular/core';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-list',
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>My Bikes</h2>
        <a routerLink="/bikes/register" class="btn btn-primary">Register New Bike</a>
      </div>

      <div class="alert alert-info" *ngIf="loading">
        Loading bikes...
      </div>

      <div class="alert alert-danger" *ngIf="error">
        {{error}}
      </div>

      <div class="alert alert-info" *ngIf="!loading && !error && bikes.length === 0">
        You don't have any bikes registered yet. Click "Register New Bike" to add one.
      </div>

      <div class="row" *ngIf="bikes.length > 0">
        <div class="col-md-4 mb-4" *ngFor="let bike of bikes">
          <div class="card h-100">
            <div class="card-header">
              <h5 class="card-title mb-0">{{bike.brandName || 'Unknown Brand'}} {{bike.model || ''}}</h5>
            </div>
            <div class="card-body">
              <p><strong>Frame Number:</strong> {{bike.frameNumber}}</p>
              <p><strong>Type:</strong> {{bike.bikeTypeName || 'Unknown Type'}}</p>
              <p><strong>Manufacturer:</strong> {{bike.manufacturerName || 'Unknown Manufacturer'}}</p>
              <p *ngIf="bike.color"><strong>Color:</strong> {{bike.color}}</p>
              <p *ngIf="bike.manufactureYear"><strong>Year:</strong> {{bike.manufactureYear}}</p>
              <p><strong>Registered:</strong> {{bike.registeredDate | date}}</p>
            </div>
            <div class="card-footer">
              <a [routerLink]="['/bikes', bike.id]" class="btn btn-primary me-2">Details</a>
              <a [routerLink]="['/bikes', bike.id, 'transfer']" class="btn btn-outline-secondary">Transfer</a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class BikeListComponent implements OnInit {
  bikes: BikeFrame[] = [];
  loading = false;
  error = '';

  constructor(private bikeService: BikeService) { }

  ngOnInit(): void {
    this.loadBikes();
  }

  loadBikes(): void {
    this.loading = true;
    this.bikeService.getBikeFrames()
      .subscribe({
        next: bikes => {
          this.bikes = bikes;
          this.loading = false;
        },
        error: error => {
          this.error = 'Failed to load bikes. Please try again later.';
          this.loading = false;
          console.error('Error loading bikes:', error);
        }
      });
  }
}
