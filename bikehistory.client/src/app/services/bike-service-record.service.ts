import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BikeServiceRecord } from '../models/bike-service-record.model';
//import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BikeServiceRecordService {
  //private apiUrl = `${environment.apiUrl}/api/BikeService`;
  private apiUrl = '/api/BikeService';

  constructor(private http: HttpClient) { }

  getAll(): Observable<BikeServiceRecord[]> {
    return this.http.get<BikeServiceRecord[]>(this.apiUrl);
  }

  getById(id: number): Observable<BikeServiceRecord> {
    return this.http.get<BikeServiceRecord>(`${this.apiUrl}/${id}`);
  }

  getByBikeFrame(bikeFrameId: number): Observable<BikeServiceRecord[]> {
    return this.http.get<BikeServiceRecord[]>(`${this.apiUrl}/bike/${bikeFrameId}`);
  }

  getByShop(shopId: string): Observable<BikeServiceRecord[]> {
    return this.http.get<BikeServiceRecord[]>(`${this.apiUrl}/shop/${shopId}`);
  }

  create(record: BikeServiceRecord): Observable<BikeServiceRecord> {
    return this.http.post<BikeServiceRecord>(this.apiUrl, record);
  }

  update(id: number, record: BikeServiceRecord): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, record);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
