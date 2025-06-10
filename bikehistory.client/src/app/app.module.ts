import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/auth/login.component';
import { RegisterComponent } from './components/auth/register.component';
import { ProfileComponent } from './components/auth/profile.component';
import { BikeDetailComponent } from './components/bikes/bike-detail.component';
import { BikeListComponent } from './components/bikes/bike-list.component';
import { BikeRegisterComponent } from './components/bikes/bike-register.component';
import { BikeTransferComponent } from './components/bikes/bike-transfer.component';
import { BrandManagementComponent } from './components/admin/brand-management.component';
import { ManufacturerManagementComponent } from './components/admin/manufacturer-management.component';
import { BikeTypeManagementComponent } from './components/admin/bike-type-management.component';
import { UserManagementComponent } from './components/admin/user-management.component'; // 추가
import { MaintenanceListComponent } from './components/maintenance/maintenance-list.component';
import { MaintenanceDetailComponent } from './components/maintenance/maintenance-detail.component';
import { MaintenanceFormComponent } from './components/maintenance/maintenance-form.component';
import { SumPipe } from './pipes/sum.pipe';

import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { AuthService } from './services/auth.service';
import { BikeService } from './services/bike.service';
import { CatalogService } from './services/catalog.service';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    ProfileComponent, 
    BikeListComponent,
    BikeDetailComponent,
    BikeRegisterComponent,
    BikeTransferComponent,
    BrandManagementComponent, 
    ManufacturerManagementComponent,
    BikeTypeManagementComponent,
    UserManagementComponent, // 추가
    MaintenanceListComponent,
    MaintenanceDetailComponent,
    MaintenanceFormComponent,
    SumPipe

  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    AuthService,
    BikeService,
    CatalogService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

