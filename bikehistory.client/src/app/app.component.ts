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
  }

  // app.component.ts에 다음 getter 추가
  get fullName(): string {
    return this.currentUser ? `${this.currentUser.firstName} ${this.currentUser.lastName}` : '';
  }
}

