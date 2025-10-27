export interface WatchHistoryItem {
  movieId: number
  episodeId?: number | null
  watchedAt: string // ISO date string
  position: number
  progress: number
}

export interface WatchlistItem {
  movieId: number
  addedAt: string // ISO date string
}


export interface User {
  userId: string
  email: string
  firstName: string | null
  lastName: string | null
  dateOfBirth: string | null
  avatar: string | null
  createdAt: string
  updatedAt: string
  watchHistory: WatchHistoryItem[]
  watchlist: WatchlistItem[]
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

export interface RefreshResponse {
  accessToken: string
}

export interface SocialLoginProvider {
  provider: "google" | "facebook"
  token: string
}
