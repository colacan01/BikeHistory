import { Injectable, Inject, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { UserActivityLog, ActivityLogFilter, UserActivityLogResponse, PaginatedResponse } from '../models/user-activity-log.model';

@Injectable({
  providedIn: 'root'
})
export class ActivityLoggerService {
  private lastUrl: string = '';
  private authService: AuthService | null = null;

  constructor(
    private http: HttpClient,
    private router: Router,
    private injector: Injector
  ) {
    // 라우터 이벤트 구독하여 페이지 이동 감지
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.logNavigation(event.urlAfterRedirects);
    });
  }

  // AuthService를 지연 로딩
  private getAuthService(): AuthService {
    if (!this.authService) {
      this.authService = this.injector.get(AuthService);
    }
    return this.authService;
  }

  private logNavigation(currentUrl: string): void {
    const auth = this.getAuthService();
    if (auth.isLoggedIn) {
      const log: UserActivityLog = {
        userId: auth.getCurrentUserId() ?? undefined,
        userName: auth.currentUser?.email,
        pageUrl: currentUrl,
        previousPageUrl: this.lastUrl,
        actionType: 'Navigation'
      };

      this.sendLog(log);
    }
    this.lastUrl = currentUrl;
  }

  public logNavigationExplicitly(targetUrl: string, additionalData?: Record<string, string>): void {
    const auth = this.getAuthService();
    if (auth.isLoggedIn) {
      const log: UserActivityLog = {
        userId: auth.getCurrentUserId() ?? undefined,
        userName: auth.currentUser?.email,
        pageUrl: targetUrl,
        previousPageUrl: this.router.url, // 현재 URL을 이전 URL로 기록
        actionType: 'ExplicitNavigation',
        additionalData: additionalData
      };

      this.sendLog(log);
    }
  }

  public logAction(actionType: string, additionalData?: Record<string, string>): void {
    const auth = this.getAuthService();
    if (auth.isLoggedIn) {
      const log: UserActivityLog = {
        userId: auth.getCurrentUserId() ?? undefined,
        userName: auth.currentUser?.email,
        pageUrl: this.router.url,
        previousPageUrl: this.lastUrl,
        actionType: actionType,
        additionalData: additionalData
      };

      this.sendLog(log);
    }
  }

  private sendLog(log: UserActivityLog): void {
    this.http.post('/api/useractivity', log).subscribe({
      next: () => {},
      error: (error) => {
        console.error('Failed to log user activity', error);
      }
    });
  }

  // 모든 활동 로그 조회 (관리자 전용)
  public getAllActivityLogs(filter: ActivityLogFilter = {}) {
    let params = new HttpParams();

    if (filter.startDate) {
      params = params.set('startDate', filter.startDate.toISOString());
    }

    if (filter.endDate) {
      params = params.set('endDate', filter.endDate.toISOString());
    }

    if (filter.page !== undefined) {
      params = params.set('page', filter.page.toString());
    }

    if (filter.pageSize !== undefined) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<PaginatedResponse<UserActivityLogResponse> | UserActivityLogResponse[]>(
      '/api/useractivity',
      { params }
    );
  }

  // 특정 사용자의 활동 로그 조회
  public getUserActivityLogs(userId: string, filter: ActivityLogFilter = {}) {
    let params = new HttpParams();

    if (filter.startDate) {
      params = params.set('startDate', filter.startDate.toISOString());
    }

    if (filter.endDate) {
      params = params.set('endDate', filter.endDate.toISOString());
    }

    if (filter.page !== undefined) {
      params = params.set('page', filter.page.toString());
    }

    if (filter.pageSize !== undefined) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<PaginatedResponse<UserActivityLogResponse> | UserActivityLogResponse[]>(
      `/api/useractivity/user/${userId}`,
      { params }
    );
  }
}

