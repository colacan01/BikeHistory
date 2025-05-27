import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login.component';
import { RegisterComponent } from './components/auth/register.component';
import { BikeDetailComponent } from './components/bikes/bike-detail.component';
import { BikeListComponent } from './components/bikes/bike-list.component';
import { BikeRegisterComponent } from './components/bikes/bike-register.component';
import { BikeTransferComponent } from './components/bikes/bike-transfer.component';
import { AuthGuard } from './guards/auth.guard';
import { ManufacturerManagementComponent } from './components/admin/manufacturer-management.component';
import { BrandManagementComponent } from './components/admin/brand-management.component';

const routes: Routes = [
  // 메인 페이지는 자전거 목록 페이지로 리다이렉트
  { path: '', redirectTo: '/bikes', pathMatch: 'full' },

  // 인증 관련 경로
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

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

  // 존재하지 않는 경로는 자전거 목록으로 리다이렉트
  { path: '**', redirectTo: '/bikes' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
