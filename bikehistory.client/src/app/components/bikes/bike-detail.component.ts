import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame, OwnershipRecord } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';
import { MaintenanceService } from '../../services/maintenance.service';
import { AuthService } from '../../services/auth.service'; // Assuming you have an AuthService to check user roles
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-bike-detail',
  templateUrl: './bike-detail.component.html',
  styles: []
})
export class BikeDetailComponent implements OnInit {
  bikeId!: number;
  bike: BikeFrame | null = null;
  ownershipHistory: OwnershipRecord[] = [];
  maintenances: any[] = [];
  loading = false;
  loadingHistory = false;
  loadingMaintenances = false;
  error = '';
  historyError = '';
  maintenanceError = '';
  isCurrentOwner = false;
  isStoreOwner = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bikeService: BikeService,
    private maintenanceService: MaintenanceService,
    private authService: AuthService,
    private activityLogger: ActivityLoggerService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.bikeId = +params['id'];

      // 자전거 상세 페이지 조회 로깅
      this.activityLogger.logAction('ViewBikeDetail', {
        bikeId: this.bikeId.toString()
      });

      this.loadBikeDetails();
      this.loadOwnershipHistory();
      this.loadMaintenances();
    });
  }

  loadBikeDetails(): void {
    this.loading = true;
    this.bikeService.getBikeFrameById(this.bikeId)
      .subscribe({
        next: bike => {
          this.bike = bike;
          this.loading = false;

          // 현재 사용자가 자전거 소유자인지 확인
          const currentUserId = this.authService.getCurrentUserId();
          this.isCurrentOwner = bike.currentOwnerId === currentUserId;

          // 현재 사용자가 정비소인지 확인
          this.isStoreOwner = this.authService.hasRole('Store');
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

  loadMaintenances(): void {
    this.loadingMaintenances = true;
    this.maintenanceService.getMaintenancesByBikeId(this.bikeId)
      .subscribe({
        next: maintenances => {
          this.maintenances = maintenances;
          this.loadingMaintenances = false;
        },
        error: error => {
          this.maintenanceError = '유지보수 기록을 불러오는데 실패했습니다.';
          this.loadingMaintenances = false;
          console.error('Error loading maintenance history:', error);
        }
      });
  }

  createNewMaintenance(): void {
    // 유지보수 생성 페이지로 이동하기 전에 명시적으로 로깅
    this.activityLogger.logNavigationExplicitly('/maintenances/new', {
      bikeId: this.bikeId.toString(),
      action: 'InitiateMaintenanceCreate'
    });

    this.router.navigate(['/maintenances/new'], {
      queryParams: { bikeFrameId: this.bikeId }
    });
  }

  // 유지보수 유형 이름 가져오기
  getMaintenanceTypeName(type: string): string {
    switch (type) {
      case 'Maintenance':
        return '정비';
      case 'Repair':
        return '수리';
      case 'Custom':
        return '커스텀';
      case 'Self':
        return 'Self';
      default:
        return type;
    }
  }

  // 결제 방법 이름 가져오기
  getPaymentMethodName(method: string): string {
    switch (method) {
      case 'Cash':
        return '현금';
      case 'LocalCurrency':
        return '지역화폐';
      case 'CreditCard':
        return '신용카드';
      case 'BankTransfer':
        return '계좌이체';
      case 'Other':
        return '기타';
      default:
        return method;
    }
  }
}
