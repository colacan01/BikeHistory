import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    if (this.authService.isLoggedIn) {
      // 경로에 역할 제한이 있는 경우
      if (route.data['roles'] && route.data['roles'].length > 0) {
        // 사용자가 필요한 역할 중 하나라도 가지고 있는지 확인
        const hasRequiredRole = this.checkUserRoles(route.data['roles']);

        if (!hasRequiredRole) {
          // 필요한 역할이 없으면 홈으로 리다이렉트
          return this.router.parseUrl('/');
        }
      }

      // 사용자가 로그인했고 필요한 역할이 있으면 접근 허용
      return true;
    }

    // 로그인하지 않은 경우, 로그인 페이지로 리다이렉트
    return this.router.parseUrl(`/login?returnUrl=${state.url}`);
  }

  // 사용자가 필요한 역할 중 하나라도 가지고 있는지 확인하는 메서드
  private checkUserRoles(requiredRoles: string[]): boolean {
    // Admin 역할 확인
    if (requiredRoles.includes('Admin') && this.authService.isAdmin) {
      return true;
    }

    // Store 역할 확인
    if (requiredRoles.includes('Store') && this.authService.isStore) {
      return true;
    }

    // 필요에 따라 다른 역할도 추가 가능

    return false;
  }
}
