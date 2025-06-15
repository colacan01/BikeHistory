import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MaintenanceService } from '../../services/maintenance.service';
import { AuthService } from '../../services/auth.service';
import { Maintenance, MaintenanceType, PaymentMethod } from '../../models/maintenance.model';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-maintenance-detail',
  templateUrl: './maintenance-detail.component.html',
  //styleUrls: ['./maintenance-detail.component.css']
})
export class MaintenanceDetailComponent implements OnInit {
  maintenance?: Maintenance;
  loading = false;
  error = '';
  isAdmin = false;
  isOwner = false;
  isStore = false;
  
  // Enum을 템플릿에서 접근하기 위한 속성
  MaintenanceType = MaintenanceType;
  PaymentMethod = PaymentMethod;
  
  constructor(
    private maintenanceService: MaintenanceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private activityLogger: ActivityLoggerService
  ) { }

  ngOnInit(): void {
    this.loading = true;
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = '유효하지 않은 유지보수 ID입니다.';
      this.loading = false;
      return;
    }

    // 사용자 권한 확인
    this.isAdmin = this.authService.hasRole('Admin');
    const currentUserId = this.authService.getCurrentUserId();

    this.maintenanceService.getMaintenanceById(id)
      .subscribe({
        next: (maintenance) => {
          // 자전거 상세 페이지 조회 로깅
          this.activityLogger.logAction('ViewMaintenanceById', {
            bikeId: this.maintenance?.bikeFrameId?.toString() ?? 'undefined',
          });

          this.maintenance = maintenance;
          this.loading = false;
          
          // 소유자 또는 정비소 여부 확인
          if (currentUserId) {
            this.isOwner = maintenance.ownerId === currentUserId;
            this.isStore = maintenance.storeId === currentUserId;
          }
        },
        error: (error) => {
          this.error = '유지보수 정보를 불러오는데 실패했습니다.';
          this.loading = false;
          console.error('Error loading maintenance details:', error);
        }
      });
  }

  editMaintenance(): void {
    if (this.maintenance) {
      this.router.navigate(['/maintenances/edit', this.maintenance.id]);
    }
  }

  deleteMaintenance(): void {
    if (!this.maintenance) return;
    
    if (confirm('정말로 이 유지보수 기록을 삭제하시겠습니까?')) {
      this.loading = true;
      this.maintenanceService.deleteMaintenance(this.maintenance.id)
        .subscribe({
          next: () => {
            // 자전거 상세 페이지 조회 로깅
            this.activityLogger.logAction('DeleteMaintenance', {
              MaintenanceId: this.maintenance?.id ?? 'undefined'
            });

            this.loading = false;
            this.router.navigate(['/maintenances']);
          },
          error: (error) => {
            this.error = '유지보수 기록 삭제에 실패했습니다.';
            this.loading = false;
            console.error('Error deleting maintenance:', error);
          }
        });
    }
  }
  
  goBack(): void {
    this.router.navigate(['/maintenances']);
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
}
