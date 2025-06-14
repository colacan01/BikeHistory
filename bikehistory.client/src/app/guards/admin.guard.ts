import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    
    // 관리자 권한 확인
    if (this.authService.isAdmin) {
      return true;
    }
    
    // 권한이 없으면 홈 페이지로 이동
    this.router.navigate(['/home']);
    return false;
  }
}
