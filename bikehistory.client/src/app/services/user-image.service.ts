import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserImage, ImageUploadResult } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class UserImageService {
  private apiUrl = '/api/UserImages';

  constructor(private http: HttpClient) { }

  getUserImages(): Observable<UserImage[]> {
    return this.http.get<UserImage[]>(this.apiUrl);
  }

  uploadUserImages(files: FileList): Observable<ImageUploadResult> {
    const formData = new FormData();
    
    for (let i = 0; i < files.length; i++) {
      formData.append('files', files[i]);
    }
    
    return this.http.post<ImageUploadResult>(`${this.apiUrl}/upload`, formData);
  }

  setProfileImage(imageId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${imageId}/set-profile`, {});
  }

  deleteImage(imageId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${imageId}`);
  }

  getImageUrl(imageId: number): string {
    return `/api/UserImages/serve/${imageId}`;
  }

  getThumbnailUrl(imageId: number, width: number = 150, height: number = 150): string {
    return `/api/UserImages/thumbnail/${imageId}?width=${width}&height=${height}`;
  }
}
