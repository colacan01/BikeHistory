import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styles: []
})
export class UserManagementComponent implements OnInit {
  userForm!: FormGroup;
  passwordForm!: FormGroup;
  users: any[] = [];
  filteredUsers: any[] = []; // 필터링된 사용자 목록을 저장할 배열
  searchTerm: string = ''; // 검색어를 저장할 변수
  //availableRoles: string[] = ['User', 'Admin'];
  availableRoles: string[] = ['User', 'Leader', 'Store', 'Admin'];
  isEditing = false;
  currentUserId?: string;
  submitted = false;
  passwordSubmitted = false;
  loading = false;
  loadingList = false;
  passwordLoading = false;
  error = '';
  success = '';
  passwordError = '';
  passwordSuccess = '';
  listError = '';
  showPasswordReset = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.initForms();
    this.loadUsers();
  }

  get f() { return this.userForm.controls; }
  get p() { return this.passwordForm.controls; }

  initForms(): void {
    this.userForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      roles: [[]],
      lockoutEnabled: [false]
    });

    this.passwordForm = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  loadUsers(): void {
    this.loadingList = true;
    this.listError = '';

    this.authService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.filteredUsers = [...this.users]; // 초기에 모든 사용자를 필터링된 목록에 복사
        this.loadingList = false;
      },
      error: (error) => {
        this.listError = '사용자 목록을 불러오는데 실패했습니다. 다시 시도해주세요.';
        this.loadingList = false;
        console.error('Error loading users:', error);
      }
    });
  }

  // 검색어로 사용자 필터링
  applyFilter(): void {
    if (!this.searchTerm || this.searchTerm.trim() === '') {
      this.filteredUsers = [...this.users]; // 검색어가 없으면 모든 사용자 표시
    } else {
      const searchTermLower = this.searchTerm.toLowerCase().trim();
      this.filteredUsers = this.users.filter(user =>
        (user.firstName && user.firstName.toLowerCase().includes(searchTermLower)) ||
        (user.lastName && user.lastName.toLowerCase().includes(searchTermLower)) ||
        (user.email && user.email.toLowerCase().includes(searchTermLower))
      );
    }
  }

  // 검색어 변경 이벤트 처리
  onSearchChange(event: any): void {
    this.searchTerm = event.target.value;
    this.applyFilter();
  }

  // 검색 초기화
  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilter();
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.userForm.invalid) {
      return;
    }

    this.loading = true;

    const userData = {
      firstName: this.f['firstName'].value,
      lastName: this.f['lastName'].value,
      email: this.f['email'].value,
      roles: this.f['roles'].value,
      lockoutEnabled: this.f['lockoutEnabled'].value
    };

    this.updateUser(this.currentUserId!, userData);
  }

  onPasswordSubmit(): void {
    this.passwordSubmitted = true;
    this.passwordError = '';
    this.passwordSuccess = '';

    if (this.passwordForm.invalid) {
      return;
    }

    this.passwordLoading = true;

    const userData = {
      firstName: this.f['firstName'].value,
      lastName: this.f['lastName'].value,
      email: this.f['email'].value,
      newPassword: this.p['newPassword'].value
    };

    this.updateUser(this.currentUserId!, userData, true);
  }

  updateUser(userId: string, userData: any, isPasswordReset: boolean = false): void {
    this.authService.updateUser(userId, userData).subscribe({
      next: () => {
        if (isPasswordReset) {
          this.passwordSuccess = '비밀번호가 성공적으로 재설정되었습니다.';
          this.passwordLoading = false;
          this.passwordSubmitted = false;
          this.passwordForm.reset();
          this.showPasswordReset = false;
        } else {
          this.success = '사용자 정보가 성공적으로 업데이트되었습니다.';
          this.loading = false;
          this.resetForm();
          this.loadUsers();
        }
      },
      error: (error) => {
        const errorMessage = error.error?.message || '사용자 정보 업데이트에 실패했습니다. 다시 시도해주세요.';
        
        if (isPasswordReset) {
          this.passwordError = errorMessage;
          this.passwordLoading = false;
        } else {
          this.error = errorMessage;
          this.loading = false;
        }
        
        console.error('Error updating user:', error);
      }
    });
  }

  editUser(user: any): void {
    this.isEditing = true;
    this.currentUserId = user.userId;
    this.userForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      roles: user.roles,
      lockoutEnabled: user.lockoutEnabled
    });
    
    window.scrollTo(0, 0);
  }

  deleteUser(userId: string): void {
    if (confirm('정말로 이 사용자를 삭제하시겠습니까? 이 작업은 되돌릴 수 없습니다.')) {
      this.authService.deleteUser(userId).subscribe({
        next: () => {
          this.success = '사용자가 성공적으로 삭제되었습니다.';
          this.loadUsers();
        },
        error: (error) => {
          this.error = error.error?.message || '사용자 삭제에 실패했습니다. 다시 시도해주세요.';
          console.error('Error deleting user:', error);
        }
      });
    }
  }

  resetForm(): void {
    this.submitted = false;
    this.isEditing = false;
    this.currentUserId = undefined;
    this.userForm.reset({
      lockoutEnabled: false,
      roles: []
    });
    this.error = '';
    this.success = '';
  }

  togglePasswordReset(): void {
    this.showPasswordReset = !this.showPasswordReset;
    if (!this.showPasswordReset) {
      this.passwordForm.reset();
      this.passwordSubmitted = false;
      this.passwordError = '';
      this.passwordSuccess = '';
    }
  }

  //getRoleBadgeClass(role: string): string {
  //  return role === 'Admin' ? 'bg-danger' : 'bg-primary';
  //}

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Admin':
        return 'bg-danger';
      case 'Store':
        return 'bg-warning';
      case 'Leader':
        return 'bg-info';
      case 'User':
      default:
        return 'bg-primary';
    }
  }

  // 역할 체크박스가 선택되었는지 확인하는 메서드
  hasRole(roleName: string): boolean {
    const roles = this.f['roles'].value;
    return roles && roles.includes(roleName);
  }

  // 역할 체크박스 변경 시 호출되는 메서드
  updateRole(role: string, event: any): void {
    const checked = event.target.checked;
    const roles = [...this.f['roles'].value] || [];

    if (checked && !roles.includes(role)) {
      // 역할 추가
      roles.push(role);
    } else if (!checked && roles.includes(role)) {
      // 역할 제거
      const index = roles.indexOf(role);
      if (index !== -1) {
        roles.splice(index, 1);
      }
    }

    this.f['roles'].setValue(roles);
  }
}
