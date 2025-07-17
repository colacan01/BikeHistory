import { Component, OnInit } from '@angular/core';
import { BikeFrame } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';
import { ActivatedRoute } from '@angular/router';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-bike-list',
  templateUrl: './bike-list.component.html',
  styleUrl: './bike-list.component.css'
})
export class BikeListComponent implements OnInit {
  bikes: BikeFrame[] = [];
  loading = false;
  error = '';
  fromUserManagement = false; // 이전 페이지가 사용자 관리 페이지인지 여부

  constructor(
    private bikeService: BikeService,
    private route: ActivatedRoute,
    private activityLogger: ActivityLoggerService
  ) { }

  ngOnInit(): void {
    const ownerId = this.route.snapshot.queryParamMap.get('ownerId');
    if (ownerId) {
      this.fromUserManagement = true;
    }

    this.activityLogger.logAction('ViewBikeList');

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

  // bike-list.component.ts에 추가할 메서드
  trackByBike(index: number, bike: BikeFrame): any {
    return bike.id || index;
  }

  // 대표 이미지 URL을 가져오는 메소드
  getPrimaryImageUrl(bike: BikeFrame): string | null {
    if (bike.primaryImage) {
      return this.bikeService.getImageUrl(bike.primaryImage.id);
    }
    return null;
  }

  // 대표 이미지가 있는지 확인하는 메소드
  hasPrimaryImage(bike: BikeFrame): boolean {
    return bike.primaryImage != null;
  }
}
