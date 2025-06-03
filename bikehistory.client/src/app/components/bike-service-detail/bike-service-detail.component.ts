import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BikeServiceRecordService } from '../../services/bike-service-record.service';
import { BikeServiceRecord, ServiceType, ServiceStatus } from '../../models/bike-service-record.model';

@Component({
  selector: 'app-bike-service-detail',
  templateUrl: './bike-service-detail.component.html',
  styleUrls: ['./bike-service-detail.component.css']
})
export class BikeServiceDetailComponent implements OnInit {
  serviceRecord: BikeServiceRecord | null = null;
  isLoading: boolean = true;
  errorMessage: string = '';
  
  constructor(
    private route: ActivatedRoute,
    private serviceRecordService: BikeServiceRecordService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadServiceRecord(id);
  }

  loadServiceRecord(id: number): void {
    this.isLoading = true;
    this.serviceRecordService.getById(id).subscribe({
      next: (data) => {
        this.serviceRecord = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = '정비 기록을 불러오는 중 오류가 발생했습니다.';
        this.isLoading = false;
        console.error(error);
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
}
