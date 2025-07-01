import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from './models/auth.model';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Bike History';
  currentUser: User | null = null;
  mobileMenuOpen = false;
  dropdowns = {
    admin: false,
    store: false,
    user: false
  };

  constructor(
    private router: Router,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout() {
    this.authService.logout();
    this.closeAllDropdowns();
    this.mobileMenuOpen = false;
  }

  get fullName(): string {
    return this.currentUser ? `${this.currentUser.firstName} ${this.currentUser.lastName}` : '';
  }

  getUserInitials(): string {
    if (!this.currentUser) return '';
    const firstInitial = this.currentUser.firstName?.charAt(0) || '';
    const lastInitial = this.currentUser.lastName?.charAt(0) || '';
    return (firstInitial + lastInitial).toUpperCase();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
    if (this.mobileMenuOpen) {
      this.closeAllDropdowns();
    }
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
    this.closeAllDropdowns();
  }

  toggleDropdown(dropdownName: string): void {
    // Close all other dropdowns
    Object.keys(this.dropdowns).forEach(key => {
      if (key !== dropdownName) {
        (this.dropdowns as any)[key] = false;
      }
    });
    
    // Toggle the requested dropdown
    (this.dropdowns as any)[dropdownName] = !(this.dropdowns as any)[dropdownName];
  }

  closeAllDropdowns(): void {
    Object.keys(this.dropdowns).forEach(key => {
      (this.dropdowns as any)[key] = false;
    });
  }

  // Close dropdowns when clicking outside
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.dropdown')) {
      this.closeAllDropdowns();
    }
  }
}
