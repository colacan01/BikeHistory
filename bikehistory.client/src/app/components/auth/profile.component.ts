import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProfileResponse, UpdateProfileRequest, UserImage, ImageUploadResult } from '../../models/auth.model';
import { AuthService } from '../../services/auth.service';
import { UserImageService } from '../../services/user-image.service';
import { ActivityLoggerService } from '../../services/activity-logger.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  profile: ProfileResponse | null = null;
  userImages: UserImage[] = [];
  submitted = false;
  passwordSubmitted = false;
  loading = false;
  passwordLoading = false;
  error = '';
  success = '';
  passwordError = '';
  passwordSuccess = '';
  showPasswordChange = false;
  
  // 이미지 관련 속성
  showImageUpload = false;
  selectedFiles: FileList | null = null;
  uploading = false;
  uploadProgress = 0;
  imageError = '';
  imageSuccess = '';
  isDragOver = false;
  loadingImages = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private userImageService: UserImageService,
    private activityLogger: ActivityLoggerService
  ) { }

  ngOnInit(): void {
    this.initForms();
    this.loadProfile();
    this.loadUserImages();
    // 자전거 상세 페이지 조회 로깅
    this.activityLogger.logAction('ViewUserProfile');
  }

  get f() { return this.profileForm.controls; }
  get p() { return this.passwordForm.controls; }

  initForms(): void {
    this.profileForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['']
    });

    this.passwordForm = this.formBuilder.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', Validators.required]
    }, {
      validators: this.mustMatch('newPassword', 'confirmNewPassword')
    });
  }

  // 비밀번호 일치 검사 유효성 검사기
  mustMatch(controlName: string, matchingControlName: string) {
    return (formGroup: FormGroup) => {
      const control = formGroup.controls[controlName];
      const matchingControl = formGroup.controls[matchingControlName];

      if (matchingControl.errors && !matchingControl.errors['mustMatch']) {
        return;
      }

      if (control.value !== matchingControl.value) {
        matchingControl.setErrors({ mustMatch: true });
      } else {
        matchingControl.setErrors(null);
      }
    };
  }

  loadProfile(): void {
    this.loading = true;
    this.error = '';

    this.authService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.userImages = data.images || [];
        this.profileForm.patchValue({
          firstName: data.firstName,
          lastName: data.lastName,
          //phoneNumber: data.phoneNumber || ''
        });
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to load profile. Please try again.';
        this.loading = false;
        console.error('Error loading profile:', error);
      }
    });
  }

  loadUserImages(): void {
    this.loadingImages = true;
    this.userImageService.getUserImages().subscribe({
      next: (images) => {
        this.userImages = images;
        this.loadingImages = false;
      },
      error: (error) => {
        this.imageError = '이미지를 불러오는데 실패했습니다.';
        this.loadingImages = false;
        console.error('Error loading user images:', error);
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.profileForm.invalid) {
      return;
    }

    this.loading = true;

    const profileData: UpdateProfileRequest = {
      firstName: this.f['firstName'].value,
      lastName: this.f['lastName'].value,
      //phoneNumber: this.f['phoneNumber'].value
    };

    this.updateProfile(profileData);
  }

  onPasswordSubmit(): void {
    this.passwordSubmitted = true;
    this.passwordError = '';
    this.passwordSuccess = '';

    if (this.passwordForm.invalid) {
      return;
    }

    this.passwordLoading = true;

    const profileData: UpdateProfileRequest = {
      firstName: this.profile?.firstName || '',
      lastName: this.profile?.lastName || '',
      //phoneNumber: this.profile?.phoneNumber,
      currentPassword: this.p['currentPassword'].value,
      newPassword: this.p['newPassword'].value,
      confirmNewPassword: this.p['confirmNewPassword'].value
    };

    this.updateProfile(profileData, true);
  }

  updateProfile(profileData: UpdateProfileRequest, isPasswordChange: boolean = false): void {
    this.authService.updateProfile(profileData).subscribe({
      next: (response) => {
        if (isPasswordChange) {
          // 자전거 상세 페이지 조회 로깅
          this.activityLogger.logAction('ChangePassword', {
            firstName: profileData.firstName,
            lastName: profileData.lastName
          });

          this.passwordSuccess = 'Password updated successfully.';
          this.passwordLoading = false;
          this.passwordSubmitted = false;
          this.passwordForm.reset();
          this.showPasswordChange = false;
        } else {
          // 자전거 상세 페이지 조회 로깅
          this.activityLogger.logAction('UpdateUserProfile', {
            firstName: profileData.firstName,
            lastName: profileData.lastName
          });

          this.success = 'Profile updated successfully.';
          this.loading = false;
          
          // 프로필 정보 다시 로드
          this.loadProfile();
        }
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'Failed to update profile. Please try again.';
        
        if (isPasswordChange) {
          this.passwordError = errorMessage;
          this.passwordLoading = false;
        } else {
          this.error = errorMessage;
          this.loading = false;
        }
        
        console.error('Error updating profile:', error);
      }
    });
  }

  togglePasswordChange(): void {
    this.showPasswordChange = !this.showPasswordChange;
    if (!this.showPasswordChange) {
      this.passwordForm.reset();
      this.passwordSubmitted = false;
      this.passwordError = '';
      this.passwordSuccess = '';
    }
  }

  // 이미지 업로드 관련 메서드
  toggleImageUpload(): void {
    this.showImageUpload = !this.showImageUpload;
    if (!this.showImageUpload) {
      this.selectedFiles = null;
      this.imageError = '';
      this.imageSuccess = '';
    }
  }

  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.selectedFiles = files;
      this.imageError = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFiles = files;
      this.imageError = '';
    }
  }

  uploadImages(): void {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      this.imageError = '업로드할 파일을 선택해주세요.';
      return;
    }

    this.uploading = true;
    this.uploadProgress = 0;
    this.imageError = '';

    this.userImageService.uploadUserImages(this.selectedFiles).subscribe({
      next: (result: ImageUploadResult) => {
        this.uploading = false;
        this.uploadProgress = 100;
        
        if (result.errors && result.errors.length > 0) {
          this.imageError = result.errors.join(', ');
        } else {
          this.imageSuccess = `${result.totalUploaded}개의 이미지가 성공적으로 업로드되었습니다.`;
        }
        
        this.selectedFiles = null;
        this.showImageUpload = false;
        this.loadUserImages();
        this.loadProfile(); // 프로필 이미지 업데이트를 위해 프로필 재로드
      },
      error: (error) => {
        this.uploading = false;
        this.uploadProgress = 0;
        this.imageError = '이미지 업로드에 실패했습니다.';
        console.error('Error uploading images:', error);
      }
    });
  }

  setProfileImage(imageId: number): void {
    this.userImageService.setProfileImage(imageId).subscribe({
      next: () => {
        this.imageSuccess = '프로필 이미지가 변경되었습니다.';
        this.loadUserImages();
        this.loadProfile();
      },
      error: (error) => {
        this.imageError = '프로필 이미지 변경에 실패했습니다.';
        console.error('Error setting profile image:', error);
      }
    });
  }

  deleteImage(imageId: number): void {
    if (confirm('정말로 이 이미지를 삭제하시겠습니까?')) {
      this.userImageService.deleteImage(imageId).subscribe({
        next: () => {
          this.imageSuccess = '이미지가 삭제되었습니다.';
          this.loadUserImages();
          this.loadProfile();
        },
        error: (error) => {
          this.imageError = '이미지 삭제에 실패했습니다.';
          console.error('Error deleting image:', error);
        }
      });
    }
  }

  getImageUrl(image: UserImage): string {
    return this.userImageService.getImageUrl(image.id);
  }

  getThumbnailUrl(image: UserImage): string {
    return this.userImageService.getThumbnailUrl(image.id, 150, 150);
  }

  getProfileImageUrl(): string | null {
    if (this.profile?.profileImage) {
      return this.userImageService.getThumbnailUrl(this.profile.profileImage.id, 120, 120);
    }
    return null;
  }

  hasProfileImage(): boolean {
    return this.profile?.profileImage != null;
  }
}
