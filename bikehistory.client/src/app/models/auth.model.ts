export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  token?: string;
}

export interface UserImage {
  id: number;
  userId: string;
  fileName: string;
  originalFileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  isProfileImage: boolean;
  uploadedDate: Date;
  description?: string;
  isDeleted: boolean;
  deletedDate?: Date;
}

export interface ImageUploadResult {
  uploadedImages: UserImage[];
  errors: string[];
  totalUploaded: number;
  totalErrors: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface ProfileResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  //phoneNumber?: string;
  roles: string[];
  images: UserImage[];
  profileImage?: UserImage;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  //phoneNumber?: string;
  currentPassword?: string;
  newPassword?: string;
  confirmNewPassword?: string;
}
