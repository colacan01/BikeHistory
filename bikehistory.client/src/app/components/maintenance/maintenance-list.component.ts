import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MaintenanceService } from '../../services/maintenance.service';
import { Maintenance, MaintenanceType, PaymentMethod } from '../../models/maintenance.model';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-maintenance-list',
  templateUrl: './maintenance-list.component.html',
  styleUrls: ['./maintenance-list.component.css']
})
export class MaintenanceListComponent implements OnInit {
  maintenances: Maintenance[] = [];
  loading = false;
  error = '';
  bikeFrameId?: number;
  ownerId?: string;
  storeId?: string;
  
  // Enum을 템플릿에서 접근하기 위한 속성
  MaintenanceType = MaintenanceType;
  PaymentMethod = PaymentMethod;

  constructor(
    private maintenanceService: MaintenanceService,
    private route: ActivatedRoute,
    private router: Router,
    private activityLogger: ActivityLoggerService
  ) { }

  ngOnInit(): void {
    // URL 매개변수 읽기
    this.route.queryParams.subscribe(params => {
      this.bikeFrameId = params['bikeFrameId'] ? +params['bikeFrameId'] : undefined;
      this.ownerId = params['ownerId'];
      this.storeId = params['storeId'];

      
      this.loadMaintenances();
    });
  }

  loadMaintenances(): void {
    this.loading = true;
    
    // bikeFrameId가 있으면 해당 자전거의 유지보수 기록만 조회
    if (this.bikeFrameId) {
      this.maintenanceService.getMaintenancesByBikeId(this.bikeFrameId)
        .subscribe({
          next: (data) => {
            // 자전거 상세 페이지 조회 로깅
            this.activityLogger.logAction('ViewMaintenanceHistoryAll', {
              bikeFrameId: this.bikeFrameId?.toString() ?? 'undefined',
              ownerId: this.ownerId ?? 'undefined',
              storeId: this.storeId ?? 'undefined'
            });

            this.maintenances = data;
            this.loading = false;
          },
          error: (error) => {
            this.error = '유지보수 기록을 불러오는데 실패했습니다.';
            this.loading = false;
            console.error('Error loading maintenances:', error);
          }
        });
    } else {
      // 그 외의 경우 필터링된 목록 조회
      this.maintenanceService.getMaintenances(this.ownerId, this.storeId)
        .subscribe({
          next: (data) => {
            this.activityLogger.logAction('ViewMaintenanceHistoryUser', {
              ownerId: this.ownerId ?? 'undefined',
              storeId: this.storeId ?? 'undefined',
            });
            this.maintenances = data;
            this.loading = false;
          },
          error: (error) => {
            this.error = '유지보수 기록을 불러오는데 실패했습니다.';
            this.loading = false;
            console.error('Error loading maintenances:', error);
          }
        });
    }
  }

  viewDetail(id: string): void {
    this.router.navigate(['/maintenances', id]);
  }

  createNew(): void {
    this.router.navigate(['/maintenances/new'], { 
      queryParams: { bikeFrameId: this.bikeFrameId }
    });
  }
  
  getMaintenanceTypeName(type: MaintenanceType): string {
    switch (type) {
      case MaintenanceType.Maintenance:
        return '정비';
      case MaintenanceType.Repair:
        return '수리';
      case MaintenanceType.Custom:
        return '커스텀';
      case MaintenanceType.Self:
        return 'Self';
      default:
        return type;
    }
  }
  
  getPaymentMethodName(method: PaymentMethod): string {
    switch (method) {
      case PaymentMethod.Cash:
        return '현금';
      case PaymentMethod.LocalCurrency:
        return '지역화폐';
      case PaymentMethod.CreditCard:
        return '신용카드';
      case PaymentMethod.BankTransfer:
        return '계좌이체';
      case PaymentMethod.Other:
        return '기타';
      default:
        return method;
    }
  }

  // maintenance-list.component.ts에 추가할 메서드
  trackByMaintenance(index: number, maintenance: Maintenance): any {
    return maintenance.id || index;
  }
}
