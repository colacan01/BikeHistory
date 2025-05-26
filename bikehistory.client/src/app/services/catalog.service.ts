import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BikeType, Brand, Manufacturer } from '../models/catalog.model';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private apiUrl = '/api/catalog';

  constructor(private http: HttpClient) { }

  // Manufacturer methods
  getManufacturers(): Observable<Manufacturer[]> {
    return this.http.get<Manufacturer[]>(`${this.apiUrl}/manufacturers`);
  }

  getManufacturerById(id: number): Observable<Manufacturer> {
    return this.http.get<Manufacturer>(`${this.apiUrl}/manufacturers/${id}`);
  }

  createManufacturer(manufacturer: Manufacturer): Observable<Manufacturer> {
    return this.http.post<Manufacturer>(`${this.apiUrl}/manufacturers`, manufacturer);
  }

  updateManufacturer(id: number, manufacturer: Manufacturer): Observable<any> {
    return this.http.put(`${this.apiUrl}/manufacturers/${id}`, manufacturer);
  }

  deleteManufacturer(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/manufacturers/${id}`);
  }

  // Brand methods
  getBrands(): Observable<Brand[]> {
    return this.http.get<Brand[]>(`${this.apiUrl}/brands`);
  }

  getBrandsByManufacturer(manufacturerId: number): Observable<Brand[]> {
    return this.http.get<Brand[]>(`${this.apiUrl}/manufacturers/${manufacturerId}/brands`);
  }

  getBrandById(id: number): Observable<Brand> {
    return this.http.get<Brand>(`${this.apiUrl}/brands/${id}`);
  }

  createBrand(brand: Brand): Observable<Brand> {
    return this.http.post<Brand>(`${this.apiUrl}/brands`, brand);
  }

  updateBrand(id: number, brand: Brand): Observable<any> {
    return this.http.put(`${this.apiUrl}/brands/${id}`, brand);
  }

  deleteBrand(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/brands/${id}`);
  }

  // BikeType methods
  getBikeTypes(): Observable<BikeType[]> {
    return this.http.get<BikeType[]>(`${this.apiUrl}/biketypes`);
  }

  getBikeTypeById(id: number): Observable<BikeType> {
    return this.http.get<BikeType>(`${this.apiUrl}/biketypes/${id}`);
  }

  createBikeType(bikeType: BikeType): Observable<BikeType> {
    return this.http.post<BikeType>(`${this.apiUrl}/biketypes`, bikeType);
  }

  updateBikeType(id: number, bikeType: BikeType): Observable<any> {
    return this.http.put(`${this.apiUrl}/biketypes/${id}`, bikeType);
  }

  deleteBikeType(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/biketypes/${id}`);
  }
}
