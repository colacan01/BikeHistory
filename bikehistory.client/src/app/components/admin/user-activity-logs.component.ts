import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivityLoggerService } from '../../services/activity-logger.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ActivityLogFilter, UserActivityLogResponse, PaginatedResponse } from '../../models/user-activity-log.model';

@Component({
  selector: 'app-user-activity-logs',
  templateUrl: './user-activity-logs.component.html',
  styles: []
})
export class UserActivityLogsComponent implements OnInit {
  logs: UserActivityLogResponse[] = [];
  loading = false;
  error = '';
  filterForm: FormGroup;
  users: any[] = [];
  selectedLog: UserActivityLogResponse | null = null; // 모달에 표시할 로그
  
  // 페이징
  currentPage = 1;
  pageSize = 20;
  totalItems = 0;
  totalPages = 1;

  constructor(
    private activityLogger: ActivityLoggerService,
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.filterForm = this.fb.group({
      startDate: [''],
      endDate: [''],
      userId: ['']
    });
  }

  ngOnInit(): void {
    // 관리자만 접근 가능하도록 체크
    if (!this.authService.isAdmin) {
      this.router.navigate(['/home']);
      return;
    }
    
    // 사용자 목록 가져오기
    this.authService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
    
    // 초기 로그 데이터 로드
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading = true;
    this.error = '';
    
    const filter: ActivityLogFilter = {
      page: this.currentPage, // 백엔드는 0부터 시작하는 페이지 인덱스를 사용할 수 있음
      pageSize: this.pageSize
    };
    
    // 필터 적용
    const formValues = this.filterForm.value;
    if (formValues.startDate) {
      filter.startDate = new Date(formValues.startDate);
    }
    
    if (formValues.endDate) {
      filter.endDate = new Date(formValues.endDate);
    }
    
    if (formValues.userId) {
      filter.userId = formValues.userId;
      
      // 특정 사용자의 로그만 조회
      this.activityLogger.getUserActivityLogs(formValues.userId, filter).subscribe({
        next: (response) => {
          if ('data' in response) {
            // PaginatedResponse 형태로 응답이 왔을 경우
            this.handlePaginatedResponse(response);
          } else {
            // 배열 형태로 응답이 왔을 경우 (이전 API와의 호환성)
            this.logs = response as UserActivityLogResponse[];
            this.totalItems = this.logs.length;
            this.totalPages = Math.ceil(this.totalItems / this.pageSize);
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = '로그 데이터를 불러오는데 실패했습니다.';
          this.loading = false;
          console.error('Error loading logs:', error);
        }
      });
    } else {
      // 모든 로그 조회
      this.activityLogger.getAllActivityLogs(filter).subscribe({
        next: (response) => {
          if ('data' in response) {
            // PaginatedResponse 형태로 응답이 왔을 경우
            this.handlePaginatedResponse(response);
          } else {
            // 배열 형태로 응답이 왔을 경우 (이전 API와의 호환성)
            this.logs = response as UserActivityLogResponse[];
            this.totalItems = this.logs.length;
            this.totalPages = Math.ceil(this.totalItems / this.pageSize);
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = '로그 데이터를 불러오는데 실패했습니다.';
          this.loading = false;
          console.error('Error loading logs:', error);
        }
      });
    }
  }

  // 페이징된 응답 처리
  private handlePaginatedResponse(response: PaginatedResponse<UserActivityLogResponse>): void {
    this.logs = response.data;
    this.totalItems = response.totalItems;
    this.totalPages = response.totalPages;
  }

  // 페이지 번호 배열 생성 (현재 페이지 주변에 표시할 페이지 번호들)
  getPageNumbers(): number[] {
    const pages: number[] = [];
    let startPage = Math.max(1, this.currentPage - 1);
    let endPage = Math.min(this.totalPages, this.currentPage + 1);

    // 범위 확장 (최소 3개 페이지 표시)
    if (endPage - startPage < 2) {
      if (startPage === 1) {
        endPage = Math.min(3, this.totalPages);
      } else {
        startPage = Math.max(1, endPage - 2);
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  applyFilter(): void {
    this.currentPage = 1; // 필터 적용 시 첫 페이지로 이동
    this.loadLogs();
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadLogs();
  }

  // 페이징 처리
  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return; // 유효하지 않은 페이지 무시
    }
    this.currentPage = page;
    this.loadLogs();
  }

  // 추가 데이터 파싱
  parseAdditionalData(json: string): Record<string, string> | null {
    try {
      return json ? JSON.parse(json) : null;
    } catch (error) {
      console.error('Error parsing JSON:', error);
      return null;
    }
  }

  // 사용자 이름 가져오기
  getUserName(userId: string): string {
    const user = this.users.find(u => u.userId === userId);
    return user ? `${user.firstName} ${user.lastName}` : 'Unknown User';
  }

  // 모달 표시 메서드
  showLogDetails(log: UserActivityLogResponse): void {
    this.selectedLog = log;
  }
}
