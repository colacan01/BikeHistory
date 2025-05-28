import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProfileResponse, UpdateProfileRequest } from '../../models/auth.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styles: []
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  profile: ProfileResponse | null = null;
  submitted = false;
  passwordSubmitted = false;
  loading = false;
  passwordLoading = false;
  error = '';
  success = '';
  passwordError = '';
  passwordSuccess = '';
  showPasswordChange = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.initForms();
    this.loadProfile();
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
          this.passwordSuccess = 'Password updated successfully.';
          this.passwordLoading = false;
          this.passwordSubmitted = false;
          this.passwordForm.reset();
          this.showPasswordChange = false;
        } else {
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
}
