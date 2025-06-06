import { Injectable } from '@angular/core';
//import { HttpClient } from '@angular/common/http';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BikeFrame, BikeFrameRegisterRequest, BikeFrameUpdateRequest, OwnershipRecord, OwnershipTransferRequest } from '../models/bike.model';

@Injectable({
  providedIn: 'root'
})
export class BikeService {
  private apiUrl = '/api/bikeframes';

  constructor(private http: HttpClient) { }

  getBikeFrames(ownerId?: string): Observable<BikeFrame[]> {
    let params = new HttpParams();
    if (ownerId) {
      params = params.append('ownerId', ownerId);
    }
    return this.http.get<BikeFrame[]>(this.apiUrl, { params });
  }

  getBikeFrameById(id: number): Observable<BikeFrame> {
    return this.http.get<BikeFrame>(`${this.apiUrl}/${id}`);
  }

  getBikeFrameByFrameNumber(frameNumber: string): Observable<BikeFrame> {
    return this.http.get<BikeFrame>(`${this.apiUrl}/framenumber/${frameNumber}`);
  }

  registerBikeFrame(bikeFrame: BikeFrameRegisterRequest): Observable<BikeFrame> {
    return this.http.post<BikeFrame>(this.apiUrl, bikeFrame);
  }

  updateBikeFrame(id: number, bikeFrame: BikeFrameUpdateRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, bikeFrame);
  }

  transferOwnership(id: number, transferRequest: OwnershipTransferRequest): Observable<OwnershipRecord> {
    return this.http.post<OwnershipRecord>(`${this.apiUrl}/${id}/transfer`, transferRequest);
  }

  getOwnershipHistory(id: number): Observable<OwnershipRecord[]> {
    return this.http.get<OwnershipRecord[]>(`${this.apiUrl}/${id}/history`);
  }
}
