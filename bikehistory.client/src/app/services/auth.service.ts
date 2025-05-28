import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.model';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { ProfileResponse, UpdateProfileRequest } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser$: Observable<User | null>;
  private tokenKey = 'auth_token';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Initialize from localStorage
    const storedToken = localStorage.getItem(this.tokenKey);
    let userData: User | null = null;

    if (storedToken) {
      try {
        // Decode token to get user data
        const decodedToken: any = jwtDecode(storedToken);
        userData = {
          id: decodedToken.nameid || decodedToken.sub,
          email: decodedToken.email,
          firstName: decodedToken.given_name || decodedToken.firstName,
          lastName: decodedToken.family_name || decodedToken.lastName,
          token: storedToken
        };
      } catch (error) {
        // Token is invalid, remove it
        localStorage.removeItem(this.tokenKey);
        console.error('Invalid token', error);
      }
    }

    this.currentUserSubject = new BehaviorSubject<User | null>(userData);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  public get isLoggedIn(): boolean {
    return !!this.currentUser;
  }

  public get authToken(): string | null {
    return this.currentUser?.token || null;
  }
  
  public get isAdmin(): boolean {
    if (!this.currentUser) return false;
    
    try {
      // If token exists, check for admin role in token
      if (this.currentUser.token) {
        const decodedToken: any = jwtDecode(this.currentUser.token);
        // Check role claim based on JWT structure
        // Common claim names for roles: 'role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        const roleClaim = decodedToken.role || 
                         decodedToken.roles || 
                         decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
                         
        if (Array.isArray(roleClaim)) {
          return roleClaim.includes('Admin');
        } else if (typeof roleClaim === 'string') {
          return roleClaim === 'Admin';
        }
      }
      return false;
    } catch (error) {
      console.error('Error checking admin status:', error);
      return false;
    }
  }

  login(loginRequest: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', loginRequest)
      .pipe(
        tap(response => {
          // Save token in localStorage
          localStorage.setItem(this.tokenKey, response.token);

          // Create user object
          const user: User = {
            id: response.userId,
            email: response.email,
            firstName: response.firstName,
            lastName: response.lastName,
            token: response.token
          };

          // Update current user subject
          this.currentUserSubject.next(user);
        })
      );
  }

  register(registerRequest: RegisterRequest): Observable<any> {
    return this.http.post('/api/auth/register', registerRequest);
  }

  logout(): void {
    // Remove token from localStorage
    localStorage.removeItem(this.tokenKey);
    
    // Set current user to null
    this.currentUserSubject.next(null);
    
    // Redirect to login page
    this.router.navigate(['/login']);
  }

  // 사용자 프로파일 조회
  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>('/api/auth/profile');
  }

  // 사용자 프로파일 업데이트
  updateProfile(profileData: UpdateProfileRequest): Observable<AuthResponse> {
    return this.http.put<AuthResponse>('/api/auth/profile', profileData)
      .pipe(
        tap(response => {
          // 토큰이 업데이트된 경우 저장
          if (response.token) {
            localStorage.setItem(this.tokenKey, response.token);

            // 사용자 객체 업데이트
            const user: User = {
              id: response.userId,
              email: response.email,
              firstName: response.firstName,
              lastName: response.lastName,
              //phoneNumber: response.phoneNumber,
              token: response.token
            };

            // 현재 사용자 주체 업데이트
            this.currentUserSubject.next(user);
          }
        })
      );
  }
}
