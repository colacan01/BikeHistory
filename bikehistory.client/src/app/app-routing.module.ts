import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login.component';
import { RegisterComponent } from './components/auth/register.component';
import { ProfileComponent } from './components/auth/profile.component'; // 추가
import { BikeDetailComponent } from './components/bikes/bike-detail.component';
import { BikeListComponent } from './components/bikes/bike-list.component';
import { BikeRegisterComponent } from './components/bikes/bike-register.component';
import { BikeTransferComponent } from './components/bikes/bike-transfer.component';
import { MaintenanceListComponent } from './components/maintenance/maintenance-list.component';
import { MaintenanceDetailComponent } from './components/maintenance/maintenance-detail.component';
import { MaintenanceFormComponent } from './components/maintenance/maintenance-form.component';
import { AuthGuard } from './guards/auth.guard';
import { ManufacturerManagementComponent } from './components/admin/manufacturer-management.component';
import { BrandManagementComponent } from './components/admin/brand-management.component';
import { BikeTypeManagementComponent } from './components/admin/bike-type-management.component';
import { UserManagementComponent } from './components/admin/user-management.component'; // 추가
import { UserActivityLogsComponent } from './components/admin/user-activity-logs.component';
import { AdminGuard } from './guards/admin.guard';

const routes: Routes = [
  // 메인 페이지는 자전거 목록 페이지로 리다이렉트
  { path: '', redirectTo: '/bikes', pathMatch: 'full' },

  // 인증 관련 경로
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] }, // 추가

  // 자전거 관련 경로 (인증 필요)
  {
    path: 'bikes',
    component: BikeListComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'bikes/register',
    component: BikeRegisterComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'bikes/:id',
    component: BikeDetailComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'bikes/:id/transfer',
    component: BikeTransferComponent,
    canActivate: [AuthGuard]
  },

  // 관리자 관련 경로 (인증 및 관리자 권한 필요)
  {
    path: 'admin/manufacturers',
    component: ManufacturerManagementComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/brands',
    component: BrandManagementComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/biketypes', // 추가
    component: BikeTypeManagementComponent, // 추가
    canActivate: [AuthGuard], // 추가
    data: { roles: ['Admin'] } // 추가
  },
  {
    path: 'admin/users', // 추가
    component: UserManagementComponent, // 추가
    canActivate: [AuthGuard], // 추가
    data: { roles: ['Admin'] } // 추가
  },
  // 유지보수 관련 라우트
  {
    path: 'maintenances',
    component: MaintenanceListComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'Store'] }
  },
  {
    path: 'maintenances/new',
    component: MaintenanceFormComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'Store'] }
  },
  {
    path: 'maintenances/edit/:id',
    component: MaintenanceFormComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'Store'] }
  },
  {
    path: 'maintenances/:id',
    component: MaintenanceDetailComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'Store'] }
  },
  {
    path: 'admin/logs',
    component: UserActivityLogsComponent,
    canActivate: [AdminGuard] // 관리자 권한 확인을 위한 가드 추가
  },
// 존재하지 않는 경로는 자전거 목록으로 리다이렉트
  { path: '**', redirectTo: '/bikes' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
