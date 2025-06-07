import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Maintenance, MaintenanceCreateDto, MaintenanceUpdateDto } from '../models/maintenance.model';

@Injectable({
  providedIn: 'root'
})
export class MaintenanceService {
  private apiUrl = '/api/maintenances';

  constructor(private http: HttpClient) { }

  // 유지보수 목록 가져오기 (필터링 가능)
  getMaintenances(ownerId?: string, storeId?: string, bikeFrameId?: number): Observable<Maintenance[]> {
    let params = new HttpParams();
    if (ownerId) {
      params = params.set('ownerId', ownerId);
    }
    if (storeId) {
      params = params.set('storeId', storeId);
    }
    if (bikeFrameId) {
      params = params.set('bikeFrameId', bikeFrameId.toString());
    }

    return this.http.get<Maintenance[]>(this.apiUrl, { params });
  }

  // 특정 자전거의 유지보수 기록 가져오기
  getMaintenancesByBikeId(bikeFrameId: number): Observable<Maintenance[]> {
    return this.http.get<Maintenance[]>(`${this.apiUrl}/bike/${bikeFrameId}`);
  }

  // 특정 유지보수 상세 가져오기
  getMaintenanceById(id: string): Observable<Maintenance> {
    return this.http.get<Maintenance>(`${this.apiUrl}/${id}`);
  }

  // 유지보수 기록 생성
  createMaintenance(maintenance: MaintenanceCreateDto): Observable<Maintenance> {
    return this.http.post<Maintenance>(this.apiUrl, maintenance);
  }

  // 유지보수 기록 수정
  updateMaintenance(id: string, maintenance: MaintenanceUpdateDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, maintenance);
  }

  // 유지보수 기록 삭제
  deleteMaintenance(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
