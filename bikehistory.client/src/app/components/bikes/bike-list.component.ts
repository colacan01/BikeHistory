import { Component, OnInit } from '@angular/core';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-bike-list',
  templateUrl: './bike-list.component.html',
  styles: []
})
export class BikeListComponent implements OnInit {
  bikes: BikeFrame[] = [];
  loading = false;
  error = '';
  fromUserManagement = false; // 이전 페이지가 사용자 관리 페이지인지 여부

  constructor(
    private bikeService: BikeService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const ownerId = this.route.snapshot.queryParamMap.get('ownerId');
    if (ownerId) {
      this.fromUserManagement = true;
    }

    this.loadBikes(ownerId);
  }

  loadBikes(ownerId?: any): void {
    this.loading = true;
    this.bikeService.getBikeFrames(ownerId)
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
