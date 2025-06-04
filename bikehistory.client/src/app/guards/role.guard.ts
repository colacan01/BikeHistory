import { Injectable } from '@angular/core';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard {
  
  constructor(private authService: AuthService, private router: Router) {}
  
  canActivate(route: ActivatedRouteSnapshot): boolean {
    const requiredRoles = route.data['roles'] as Array<string>;
    
    //if (!this.authService.isAuthenticated()) {
    //  this.router.navigate(['/login']);
    //  return false;
    //}

    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/login']);
      return false;
    }

    const userRoles = this.authService.getUserRoles();
    const hasRequiredRole = requiredRoles.some(role => userRoles.includes(role));
    
    if (!hasRequiredRole) {
      this.router.navigate(['/unauthorized']);
      return false;
    }
    
    return true;
  }
}
