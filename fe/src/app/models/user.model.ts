export interface User {
  userId: string
  email: string
  firstName: string | null
  lastName: string | null
  dateOfBirth: string | null
  avatar: string | null
  createdAt: string
  updatedAt: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
}

export interface UpdateProfileRequest {
  firstName: string | null
  lastName: string | null
  dateOfBirth: string | null // DateOnly format: YYYY-MM-DD
  avatar: File | null
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface AuthResponse {
  user: User
  accessToken: string,
  refreshToken?: string | null,
  accessTokenExpiresAtUtc?: string | null
}

export interface SocialLoginProvider {
  provider: "google" | "facebook"
  token: string
}
