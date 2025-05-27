import { Component, OnInit } from '@angular/core';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';

@Component({
  selector: 'app-bike-list',
  templateUrl: './bike-list.component.html',
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
