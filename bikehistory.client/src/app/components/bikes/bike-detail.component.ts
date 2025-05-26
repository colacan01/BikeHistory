import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame, OwnershipRecord } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-detail',
  template: `
    <div class="container mt-4">
      <div class="alert alert-info" *ngIf="loading">
        Loading bike details...
      </div>

      <div class="alert alert-danger" *ngIf="error">
        {{error}}
      </div>

      <div *ngIf="bike && !loading">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h2>{{bike.brandName || 'Unknown Brand'}} {{bike.model || 'Bike'}} Details</h2>
          <div>
            <a [routerLink]="['/bikes', bike.id, 'transfer']" class="btn btn-primary me-2">Transfer Ownership</a>
            <a [routerLink]="['/bikes', bike.id, 'edit']" class="btn btn-outline-secondary">Edit</a>
          </div>
        </div>

        <div class="row">
          <div class="col-md-6">
            <div class="card mb-4">
              <div class="card-header">
                <h5 class="card-title mb-0">Bike Information</h5>
              </div>
              <div class="card-body">
                <p><strong>Frame Number:</strong> {{bike.frameNumber}}</p>
                <p><strong>Type:</strong> {{bike.bikeTypeName || 'Unknown Type'}}</p>
                <p><strong>Manufacturer:</strong> {{bike.manufacturerName || 'Unknown Manufacturer'}}</p>
                <p><strong>Brand:</strong> {{bike.brandName || 'Unknown Brand'}}</p>
                <p *ngIf="bike.model"><strong>Model:</strong> {{bike.model}}</p>
                <p *ngIf="bike.manufactureYear"><strong>Year:</strong> {{bike.manufactureYear}}</p>
                <p *ngIf="bike.color"><strong>Color:</strong> {{bike.color}}</p>
                <p><strong>Registered Date:</strong> {{bike.registeredDate | date}}</p>
                <p><strong>Current Owner:</strong> {{bike.currentOwnerName || 'You'}}</p>
              </div>
            </div>
          </div>

          <div class="col-md-6">
            <div class="card mb-4">
              <div class="card-header">
                <h5 class="card-title mb-0">Ownership History</h5>
              </div>
              <div class="card-body">
                <div class="alert alert-info" *ngIf="loadingHistory">
                  Loading ownership history...
                </div>
                <div class="alert alert-danger" *ngIf="historyError">
                  {{historyError}}
                </div>
                <div *ngIf="!loadingHistory && !historyError">
                  <div *ngIf="ownershipHistory.length === 0" class="alert alert-info">
                    No ownership transfers have been recorded yet.
                  </div>
                  <ul class="list-group" *ngIf="ownershipHistory.length > 0">
                    <li class="list-group-item" *ngFor="let record of ownershipHistory">
                      <div><strong>Date:</strong> {{record.transferDate | date}}</div>
                      <div><strong>From:</strong> {{record.previousOwnerName}}</div>
                      <div><strong>To:</strong> {{record.newOwnerName}}</div>
                      <div *ngIf="record.notes"><strong>Notes:</strong> {{record.notes}}</div>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="mt-3">
          <a routerLink="/bikes" class="btn btn-outline-secondary">Back to My Bikes</a>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class BikeDetailComponent implements OnInit {
  bikeId!: number;
  bike: BikeFrame | null = null;
  ownershipHistory: OwnershipRecord[] = [];
  loading = false;
  loadingHistory = false;
  error = '';
  historyError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bikeService: BikeService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.bikeId = +params['id'];
      this.loadBikeDetails();
      this.loadOwnershipHistory();
    });
  }

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

  loadOwnershipHistory(): void {
    this.loadingHistory = true;
    this.bikeService.getOwnershipHistory(this.bikeId)
      .subscribe({
        next: history => {
          this.ownershipHistory = history;
          this.loadingHistory = false;
        },
        error: error => {
          this.historyError = 'Failed to load ownership history.';
          this.loadingHistory = false;
          console.error('Error loading ownership history:', error);
        }
      });
  }
}
