import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame, OwnershipRecord } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-detail',
  templateUrl: './bike-detail.component.html',
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
