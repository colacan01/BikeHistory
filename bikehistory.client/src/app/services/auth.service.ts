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
    private router: Router,
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

  /**
 * 현재 로그인된 사용자의 ID를 반환합니다
 * @returns 사용자 ID, 로그인되지 않은 경우 null 반환
 */
  public getCurrentUserId(): string | null {
    return this.currentUser?.id || null;
  }

  public get isLoggedIn(): boolean {
    return !!this.currentUser;
  }

  public get authToken(): string | null {
    return this.currentUser?.token || null;
  }

  public get isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  public get isStore(): boolean {
    return this.hasRole('Store');
  }

  public get isLeader(): boolean {
    return this.hasRole('Leader');
  }

  /**
 * 현재 사용자의 역할 목록을 가져옵니다
 * @returns 사용자의 역할 배열, 로그인되지 않은 경우 빈 배열 반환
 */
  public getUserRoles(): string[] {
    if (!this.currentUser || !this.currentUser.token) return [];

    try {
      const decodedToken: any = jwtDecode(this.currentUser.token);
      // 일반적인 JWT 역할 클레임 이름 검사
      const roleClaim = decodedToken.role ||
        decodedToken.roles ||
        decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      // 역할이 배열인 경우
      if (Array.isArray(roleClaim)) {
        return roleClaim;
      }
      // 역할이 단일 문자열인 경우
      else if (typeof roleClaim === 'string') {
        return [roleClaim];
      }

      return [];
    } catch (error) {
      console.error('Error getting user roles:', error);
      return [];
    }
  }

  /**
   * 사용자가 특정 역할을 가지고 있는지 확인합니다
   * @param role 확인할 역할
   * @returns 사용자가 해당 역할을 가지고 있으면 true, 그렇지 않으면 false
   */
  public hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
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

  // 사용자 목록 조회 (관리자 전용)
  getUsers(): Observable<any[]> {
    return this.http.get<any[]>('/api/auth/users');
  }

  // 특정 사용자 조회 (관리자 전용)
  getUserById(userId: string): Observable<any> {
    return this.http.get<any>(`/api/auth/users/${userId}`);
  }

  // 사용자 정보 업데이트 (관리자 전용)
  updateUser(userId: string, userData: any): Observable<any> {
    return this.http.put<any>(`/api/auth/users/${userId}`, userData);
  }

  // 사용자 삭제 (관리자 전용)
  deleteUser(userId: string): Observable<any> {
    return this.http.delete<any>(`/api/auth/users/${userId}`);
  }
}
