import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeServiceRecordService } from '../../services/bike-service-record.service';
import { BikeService } from '../../services/bike.service';
import { AuthService } from '../../services/auth.service';
import { BikeServiceRecord, ServiceType, ServiceStatus } from '../../models/bike-service-record.model';
import { BikeFrame } from '../../models/bike.model';

@Component({
  selector: 'app-bike-service-form',
  templateUrl: './bike-service-form.component.html',
  styleUrls: ['./bike-service-form.component.css']
})
export class BikeServiceFormComponent implements OnInit {
  serviceForm: FormGroup;
  isEditMode: boolean = false;
  recordId: number | null = null;
  isLoading: boolean = false;
  bikeFrames: BikeFrame[] = [];
  serviceTypes = Object.keys(ServiceType).filter(key => isNaN(Number(key)));
  serviceStatuses = Object.keys(ServiceStatus).filter(key => isNaN(Number(key)));
  errorMessage: string = '';
  
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private serviceRecordService: BikeServiceRecordService,
    private bikeService: BikeService,
    private authService: AuthService
  ) {
    this.serviceForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadBikeFrames();
    
    // ID가 있으면 수정 모드로 간주
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.recordId = Number(id);
      this.loadServiceRecord(this.recordId);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      serviceDate: [new Date(), Validators.required],
      bikeFrameId: ['', Validators.required],
      serviceType: [ServiceType.Maintenance, Validators.required],
      serviceDetails: ['', Validators.required],
      serviceShopId: [this.authService.getCurrentUserId(), Validators.required],
      cost: [null],
      partDetails: [''],
      warrantyInfo: [''],
      serviceStatus: [ServiceStatus.Completed, Validators.required],
      nextServiceDate: [null]
    });
  }

  loadBikeFrames(): void {
    this.bikeService.getAllBikeFrames().subscribe({
      next: (data) => {
        this.bikeFrames = data;
      },
      error: (error) => {
        console.error('자전거 프레임 목록을 불러오는 중 오류가 발생했습니다:', error);
      }
    });
  }

  loadServiceRecord(id: number): void {
    this.isLoading = true;
    this.serviceRecordService.getById(id).subscribe({
      next: (record) => {
        this.serviceForm.patchValue({
          serviceDate: new Date(record.serviceDate),
          bikeFrameId: record.bikeFrameId,
          serviceType: record.serviceType,
          serviceDetails: record.serviceDetails,
          serviceShopId: record.serviceShopId,
          cost: record.cost,
          partDetails: record.partDetails,
          warrantyInfo: record.warrantyInfo,
          serviceStatus: record.serviceStatus,
          nextServiceDate: record.nextServiceDate ? new Date(record.nextServiceDate) : null
        });
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = '정비 기록을 불러오는 중 오류가 발생했습니다.';
        this.isLoading = false;
        console.error(error);
      }
    });
  }

  onSubmit(): void {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      return;
    }

    const formData = this.serviceForm.value;
    this.isLoading = true;

    if (this.isEditMode && this.recordId) {
      const updatedRecord: BikeServiceRecord = {
        ...formData,
        id: this.recordId
      };
      
      this.serviceRecordService.update(this.recordId, updatedRecord).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/service-records', this.recordId]);
        },
        error: (error) => {
          this.errorMessage = '정비 기록 업데이트 중 오류가 발생했습니다.';
          this.isLoading = false;
          console.error(error);
        }
      });
    } else {
      this.serviceRecordService.create(formData).subscribe({
        next: (createdRecord) => {
          this.isLoading = false;
          this.router.navigate(['/service-records', createdRecord.id]);
        },
        error: (error) => {
          this.errorMessage = '정비 기록 생성 중 오류가 발생했습니다.';
          this.isLoading = false;
          console.error(error);
        }
      });
    }
  }
}
