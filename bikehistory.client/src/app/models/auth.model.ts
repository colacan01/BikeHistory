export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  token?: string;
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
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  //phoneNumber?: string;
  currentPassword?: string;
  newPassword?: string;
  confirmNewPassword?: string;
}
