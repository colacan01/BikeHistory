import { Component, OnInit } from '@angular/core';
import { BikeServiceRecordService } from '../../services/bike-service-record.service';
import { BikeServiceRecord, ServiceType, ServiceStatus } from '../../models/bike-service-record.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-bike-service-list',
  templateUrl: './bike-service-list.component.html',
  styleUrls: ['./bike-service-list.component.css']
})
export class BikeServiceListComponent implements OnInit {
  serviceRecords: BikeServiceRecord[] = [];
  isLoading: boolean = true;
  isAdmin: boolean = false;
  
  constructor(
    private serviceRecordService: BikeServiceRecordService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.isAdmin = this.authService.getUserRoles().includes('Admin');
    this.loadServiceRecords();
  }

  loadServiceRecords(): void {
    this.isLoading = true;
    this.serviceRecordService.getAll().subscribe({
      next: (data) => {
        this.serviceRecords = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('정비 기록을 불러오는 중 오류가 발생했습니다:', error);
        this.isLoading = false;
      }
    });
  }

  getServiceTypeName(type: ServiceType): string {
    switch(type) {
      case ServiceType.Maintenance: return '유지보수';
      case ServiceType.Repair: return '수리';
      case ServiceType.Inspection: return '점검';
      case ServiceType.Upgrade: return '업그레이드';
      case ServiceType.Custom: return '맞춤 서비스';
      default: return '알 수 없음';
    }
  }

  getServiceStatusName(status: ServiceStatus): string {
    switch(status) {
      case ServiceStatus.Scheduled: return '예약됨';
      case ServiceStatus.InProgress: return '진행 중';
      case ServiceStatus.Completed: return '완료됨';
      case ServiceStatus.Canceled: return '취소됨';
      default: return '알 수 없음';
    }
  }

  deleteRecord(id: number): void {
    if (confirm('이 정비 기록을 삭제하시겠습니까?')) {
      this.serviceRecordService.delete(id).subscribe({
        next: () => {
          this.loadServiceRecords();
        },
        error: (error) => {
          console.error('정비 기록 삭제 중 오류가 발생했습니다:', error);
        }
      });
    }
  }
}
