import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BikeFrame, BikeFrameRegisterRequest, BikeFrameUpdateRequest, OwnershipRecord, OwnershipTransferRequest, BikeImage, ImageUploadResult } from '../models/bike.model';

@Injectable({
  providedIn: 'root'
})
export class BikeService {
  private apiUrl = '/api/bikeframes';
  private imageApiUrl = '/api/BikeImages';

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

  // 이미지 관련 메서드들
  getBikeImages(bikeFrameId: number): Observable<BikeImage[]> {
    return this.http.get<BikeImage[]>(`${this.imageApiUrl}/bike/${bikeFrameId}`);
  }

  uploadBikeImages(bikeFrameId: number, files: FileList): Observable<ImageUploadResult> {
    const formData = new FormData();
    
    for (let i = 0; i < files.length; i++) {
      formData.append('files', files[i]);
    }
    
    return this.http.post<ImageUploadResult>(`${this.imageApiUrl}/upload/${bikeFrameId}`, formData);
  }

  setPrimaryImage(imageId: number): Observable<any> {
    return this.http.put(`${this.imageApiUrl}/${imageId}/set-primary`, {});
  }

  deleteImage(imageId: number): Observable<any> {
    return this.http.delete(`${this.imageApiUrl}/${imageId}`);
  }

  downloadImage(imageId: number): Observable<Blob> {
    return this.http.get(`${this.imageApiUrl}/${imageId}/download`, { responseType: 'blob' });
  }

  // Updated method to use the new serve endpoint
  getImageUrl(imageId: number): string {
    return `/api/BikeImages/serve/${imageId}`;
  }

  // New method for getting thumbnails
  getThumbnailUrl(imageId: number, width: number = 300, height: number = 300): string {
    return `/api/BikeImages/thumbnail/${imageId}?width=${width}&height=${height}`;
  }

  // Legacy method for backward compatibility
  getImageUrlByPath(filePath: string): string {
    return `/${filePath}`;
  }
}
