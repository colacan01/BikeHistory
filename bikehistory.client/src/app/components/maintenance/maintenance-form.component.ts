import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MaintenanceService } from '../../services/maintenance.service';
import { BikeService } from '../../services/bike.service';
import { AuthService } from '../../services/auth.service';
import { MaintenanceType, PaymentMethod, MaintenanceCreateDto, MaintenanceUpdateDto } from '../../models/maintenance.model';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-maintenance-form',
  templateUrl: './maintenance-form.component.html',
  styleUrls: ['./maintenance-form.component.css']
})
export class MaintenanceFormComponent implements OnInit {
  maintenanceForm: FormGroup;
  loading = false;
  error = '';
  isEditMode = false;
  maintenanceId = '';
  bikeFrameId?: number;
  submitting = false;
  
  bikes: any[] = [];
  owners: any[] = [];
  stores: any[] = [];
  
  maintenanceTypes = Object.values(MaintenanceType);
  paymentMethods = Object.values(PaymentMethod);
  
  constructor(
    private fb: FormBuilder,
    private maintenanceService: MaintenanceService,
    private bikeService: BikeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private activityLogger: ActivityLoggerService
  ) {
    this.maintenanceForm = this.createForm();
  }

  ngOnInit(): void {
    // 현재 날짜를 YYYY-MM-DD 형식으로 설정
    this.maintenanceForm.get('maintenanceDate')?.setValue(this.formatDateForInput(new Date()));

    // 현재 로그인한 사용자를 정비소로 설정
    const currentUserId = this.authService.getCurrentUserId();
    if (currentUserId) {
      this.maintenanceForm.get('storeId')?.setValue(currentUserId);
      this.maintenanceForm.get('storeId')?.disable(); // 정비소 필드를 비활성화
    }

    // URL에서 파라미터 가져오기
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.maintenanceId = params['id'];

        // 자전거 상세 페이지 조회 로깅
        this.activityLogger.logAction('ViewEditMaintenanceById', {
          mMaintenanceId: this.maintenanceId.toString() ?? 'undefined',
        });

        this.loadMaintenance(this.maintenanceId);
      }
    });
    
    this.route.queryParams.subscribe(params => {
      if (params['bikeFrameId']) {
        this.bikeFrameId = +params['bikeFrameId'];
        this.maintenanceForm.patchValue({ bikeFrameId: this.bikeFrameId });
        
        // 자전거 ID가 있으면 해당 자전거의 소유자 정보도 가져옴
        this.loadBikeOwnerInfo(this.bikeFrameId);
      }
    });
    
    // 필요한 데이터 로드
    this.loadBikes();
    this.loadUsers();
  }

  // YYYY-MM-DD 형식으로 날짜를 포맷팅하는 메서드
  formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  createForm(): FormGroup {
    return this.fb.group({
      maintenanceDate: [new Date(), Validators.required],
      maintenanceType: [MaintenanceType.Maintenance, Validators.required],
      bikeFrameId: [null, Validators.required],
      storeId: ['', Validators.required],
      ownerId: ['', Validators.required],
      paymentMethod: [PaymentMethod.Cash, Validators.required],
      notes: [''],
      details: this.fb.array([this.createDetailForm()])
    });
  }
  
  createDetailForm(): FormGroup {
    return this.fb.group({
      laborCost: [0, [Validators.required, Validators.min(0)]],
      partPrice: [0, [Validators.required, Validators.min(0)]],
      partName: [''],
      partSpecification: ['']
    });
  }
  
  get details(): FormArray {
    return this.maintenanceForm.get('details') as FormArray;
  }
  
  addDetail(): void {
    this.details.push(this.createDetailForm());
  }
  
  removeDetail(index: number): void {
    if (this.details.length > 1) {
      this.details.removeAt(index);
    }
  }
  
  loadMaintenance(id: string): void {
    this.loading = true;
    this.maintenanceService.getMaintenanceById(id).subscribe({
      next: (maintenance) => {
        // 유지보수 정보로 폼 초기화
        const maintenanceDate = new Date(maintenance.maintenanceDate);

        // 유지보수 정보로 폼 초기화
        this.maintenanceForm.patchValue({
          maintenanceDate: this.formatDateForInput(maintenanceDate), // 날짜 포맷팅
          maintenanceType: maintenance.maintenanceType,
          bikeFrameId: maintenance.bikeFrameId,
          storeId: maintenance.storeId,
          ownerId: maintenance.ownerId,
          paymentMethod: maintenance.paymentMethod,
          notes: maintenance.notes || ''
        });
        
        // 상세 정보 폼 초기화
        this.details.clear();
        if (maintenance.maintenanceDetails && maintenance.maintenanceDetails.length > 0) {
          for (const detail of maintenance.maintenanceDetails) {
            this.details.push(this.fb.group({
              laborCost: [detail.laborCost, [Validators.required, Validators.min(0)]],
              partPrice: [detail.partPrice, [Validators.required, Validators.min(0)]],
              partName: [detail.partName || ''],
              partSpecification: [detail.partSpecification || '']
            }));
          }
        } else {
          this.addDetail();
        }
        
        this.loading = false;
      },
      error: (error) => {
        this.error = '유지보수 정보를 불러오는데 실패했습니다.';
        this.loading = false;
        console.error('Error loading maintenance:', error);
      }
    });
  }
  
  loadBikes(): void {
    this.bikeService.getBikeFrames().subscribe({
      next: (bikes) => {
        this.bikes = bikes;
      },
      error: (error) => {
        console.error('Error loading bikes:', error);
      }
    });
  }
  
  loadUsers(): void {
    this.authService.getUsers().subscribe({
      next: (users) => {
        this.stores = users;
        this.owners = users;
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }
  
  loadBikeOwnerInfo(bikeId: number): void {
    this.bikeService.getBikeFrameById(bikeId).subscribe({
      next: (bike) => {
        this.maintenanceForm.patchValue({
          ownerId: bike.currentOwnerId
        });
      },
      error: (error) => {
        console.error('Error loading bike owner info:', error);
      }
    });
  }
  
  onSubmit(): void {
    if (this.maintenanceForm.invalid) {
      this.markFormGroupTouched(this.maintenanceForm);
      return;
    }
    
    this.submitting = true;

    // 비활성화된 필드 값 가져오기
    const storeId = this.maintenanceForm.get('storeId')?.value || this.authService.getCurrentUserId();

    if (this.isEditMode) {
      const updateDto: MaintenanceUpdateDto = {
        id: this.maintenanceId,
        maintenanceDate: this.maintenanceForm.value.maintenanceDate,
        maintenanceType: this.maintenanceForm.value.maintenanceType,
        paymentMethod: this.maintenanceForm.value.paymentMethod,
        notes: this.maintenanceForm.value.notes,
        details: this.maintenanceForm.value.details
      };
      
      this.maintenanceService.updateMaintenance(this.maintenanceId, updateDto).subscribe({
        next: () => {
          this.submitting = false;
         
          this.activityLogger.logAction('UpdateMaintenance', {
            maintenanceId: this.maintenanceId,
            storeId: this.maintenanceForm.value.storeId,
            ownerId: this.maintenanceForm.value.ownerId
          });

          this.router.navigate(['/maintenances', this.maintenanceId]);
        },
        error: (error) => {
          this.error = '유지보수 정보 수정에 실패했습니다.';
          this.submitting = false;
          console.error('Error updating maintenance:', error);
        }
      });
    } else {
      const createDto: MaintenanceCreateDto = {
        maintenanceDate: this.maintenanceForm.value.maintenanceDate,
        maintenanceType: this.maintenanceForm.value.maintenanceType,
        bikeFrameId: this.maintenanceForm.value.bikeFrameId,
        storeId: storeId, // 비활성화된 필드이므로 직접 값을 설정
        ownerId: this.maintenanceForm.value.ownerId,
        paymentMethod: this.maintenanceForm.value.paymentMethod,
        notes: this.maintenanceForm.value.notes,
        details: this.maintenanceForm.value.details
      };
      
      this.maintenanceService.createMaintenance(createDto).subscribe({
        next: (result) => {
          this.submitting = false;

          this.activityLogger.logAction('CreateMaintenance', {
            maintenanceId: result.id,
            storeId: createDto.storeId,
            ownerId: createDto.ownerId
          });

          this.router.navigate(['/maintenances', result.id]);
        },
        error: (error) => {
          this.error = '유지보수 정보 생성에 실패했습니다.';
          this.submitting = false;
          console.error('Error creating maintenance:', error);
        }
      });
    }
  }
  
  cancel(): void {

    // 유지보수 생성 또는 수정 취소 시 로깅
    this.activityLogger.logAction('CancelMaintenanceForm', {
      maintenanceId: this.maintenanceId || 'new',
      isEditMode: this.isEditMode.toString()
    });

    if (this.isEditMode) {
      this.router.navigate(['/maintenances', this.maintenanceId]);
    } else {
      this.router.navigate(['/maintenances']);
    }
  }
  
  calculateTotal(type: 'laborCost' | 'partPrice' | 'total'): number {
    let total = 0;
    for (const detail of this.details.controls) {
      if (type === 'laborCost') {
        total += detail.get('laborCost')?.value || 0;
      } else if (type === 'partPrice') {
        total += detail.get('partPrice')?.value || 0;
      } else if (type === 'total') {
        total += (detail.get('laborCost')?.value || 0) + (detail.get('partPrice')?.value || 0);
      }
    }
    return total;
  }
  
  // 폼 유효성 검사를 위한 헬퍼 메서드
  markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control instanceof FormArray) {
        for (const ctrl of control.controls) {
          if (ctrl instanceof FormGroup) {
            this.markFormGroupTouched(ctrl);
          }
        }
      }
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
}
