import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BikeFrame, OwnershipRecord, BikeImage } from '../../models/bike.model';
import { BikeService } from '../../services/bike.service';
import { MaintenanceService } from '../../services/maintenance.service';
import { AuthService } from '../../services/auth.service';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-bike-detail',
  templateUrl: './bike-detail.component.html',
  styleUrl: './bike-detail.component.css'
})
export class BikeDetailComponent implements OnInit {
  bikeId!: number;
  bike: BikeFrame | null = null;
  ownershipHistory: OwnershipRecord[] = [];
  maintenances: any[] = [];
  bikeImages: BikeImage[] = [];
  loading = false;
  loadingHistory = false;
  loadingMaintenances = false;
  loadingImages = false;
  error = '';
  historyError = '';
  maintenanceError = '';
  imageError = '';
  isCurrentOwner = false;
  isStoreOwner = false;
  
  // 이미지 업로드 관련
  selectedFiles: FileList | null = null;
  uploading = false;
  uploadProgress = 0;
  showImageUpload = false;
  isDragOver = false;

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
      this.loadBikeImages();
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

  loadBikeImages(): void {
    this.loadingImages = true;
    this.bikeService.getBikeImages(this.bikeId)
      .subscribe({
        next: images => {
          this.bikeImages = images;
          this.loadingImages = false;
        },
        error: error => {
          this.imageError = '이미지를 불러오는데 실패했습니다.';
          this.loadingImages = false;
          console.error('Error loading bike images:', error);
        }
      });
  }

  // 이미지 업로드 관련 메서드들
  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.selectedFiles = files;
    }
  }

  // Drag and drop handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFiles = files;
    }
  }

  uploadImages(): void {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      return;
    }

    this.uploading = true;
    this.uploadProgress = 0;

    // Simulate progress for better UX
    const progressInterval = setInterval(() => {
      if (this.uploadProgress < 90) {
        this.uploadProgress += 10;
      }
    }, 200);

    this.bikeService.uploadBikeImages(this.bikeId, this.selectedFiles)
      .subscribe({
        next: result => {
          clearInterval(progressInterval);
          this.uploadProgress = 100;
          
          setTimeout(() => {
            this.uploading = false;
            this.uploadProgress = 0;
            this.selectedFiles = null;
            this.showImageUpload = false;

            if (result.errors && result.errors.length > 0) {
              console.warn('일부 이미지 업로드 실패:', result.errors);
              this.imageError = `${result.totalUploaded}개 이미지 업로드 성공, ${result.totalErrors}개 실패`;
            } else {
              this.imageError = '';
            }

            // 이미지 목록 새로고침
            this.loadBikeImages();
          }, 500);
        },
        error: error => {
          clearInterval(progressInterval);
          this.uploading = false;
          this.uploadProgress = 0;
          this.imageError = '이미지 업로드에 실패했습니다.';
          console.error('Error uploading images:', error);
        }
      });
  }

  setPrimaryImage(imageId: number): void {
    this.bikeService.setPrimaryImage(imageId)
      .subscribe({
        next: () => {
          // 이미지 목록 새로고침
          this.loadBikeImages();
        },
        error: error => {
          console.error('Error setting primary image:', error);
        }
      });
  }

  deleteImage(imageId: number): void {
    if (confirm('이 이미지를 삭제하시겠습니까?')) {
      this.bikeService.deleteImage(imageId)
        .subscribe({
          next: () => {
            // 이미지 목록 새로고침
            this.loadBikeImages();
          },
          error: error => {
            console.error('Error deleting image:', error);
          }
        });
    }
  }

  // Updated method to use thumbnails for better performance
  getImageUrl(image: BikeImage): string {
    return this.bikeService.getThumbnailUrl(image.id, 300, 300);
  }

  // Method to get full-size image URL
  getFullImageUrl(image: BikeImage): string {
    return this.bikeService.getImageUrl(image.id);
  }

  // Method to handle image load errors
  onImageError(event: any): void {
    console.error('Image failed to load:', event);
    // Try to load the full image as fallback
    const img = event.target as HTMLImageElement;
    const imageId = img.getAttribute('data-image-id');
    if (imageId && !img.src.includes('/serve/')) {
      img.src = this.bikeService.getImageUrl(parseInt(imageId));
    } else {
      // Show a default "no image" placeholder
      img.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjMwMCIgdmlld0JveD0iMCAwIDMwMCAzMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSMzMDAiIGhlaWdodD0iMzAwIiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik0xMDAgMTAwSDE2MFYxMTBIMTAwVjEwMFoiIGZpbGw9IiM5Q0EzQUYiLz4KPHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeD0iMTMwIiB5PSIxMzAiPgo8cGF0aCBkPSJNMTIgMkM2LjQ3NyAyIDIgNi40NzcgMiAxMkMyIDE3LjUyMyA2LjQ3NyAyMiAxMiAyMkMxNzUuNTIzIDIyIDE3LjUyMyAyMiAxMiAyME0xMiAyMEM4LjUyMyAyMCA0IDE2LjQxMSA0IDEyQzQgNy41ODkgNy41ODkgNCAxMiA0QzExIDQuOTk3IDAgMCAxMS45OTQgMCIgZmlsbD0iIzlDQTNBRiIvPgogIDxpbWcgZmlsbD0icGF0dGVybiIgcmVsYXRpdmU9InRydWUiIHg9IjEyIiB5PSIxNDAiIHdpZHRoPSIxMDAiIGhlaWdodD0iIj4KICA8Y2lyY2xlIGN4PSI0MCIgY3k9IjQwIiByPSI1MCIgZmlsbD0icGF0dGVybiIgLz4KIDwvZz4KPC9zdmc+';
    }
  }

  // Method to open image in full size
  openFullImage(image: BikeImage): void {
    window.open(this.getFullImageUrl(image), '_blank');
  }

  toggleImageUpload(): void {
    this.showImageUpload = !this.showImageUpload;
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
