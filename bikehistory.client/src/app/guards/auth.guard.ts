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
      // Check if route has admin role restriction
      if (route.data['roles'] && route.data['roles'].includes('Admin') && !this.authService.isAdmin) {
        // User is not admin, redirect to home
        return this.router.parseUrl('/');
      }
      
      // User is logged in and has required role (if any)
      return true;
    }
    
    // Not logged in, redirect to login with return url
    return this.router.parseUrl(`/login?returnUrl=${state.url}`);
  }
}
