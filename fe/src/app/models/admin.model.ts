export interface AdminDashboardStats {
  kpis: {
    totalMovies: number
    publishedMovies: number
    draftMovies: number
    totalEpisodes: number
    episodesMissingVideo: number
    totalImages: number
    totalVideos: number
    totalAssets: number
    imageBytes: number
    videoBytes: number
    totalBytes: number
    orphanAssets: number
  }

  taxonomy: {
    genres: number
    studios: number
  }

  recentMovies: DashboardMovie[]
  topViewedMovies: DashboardMovie[]
  issues: DashboardIssue[]
  orphanImages: DashboardImage[]
}

export interface DashboardMovie {
  id: number
  slug: string
  title: string
  posterUrl: string
  type: "Single" | "Series"
  status: "Published" | "Draft" | "Hidden" | "InReview"
  avgRating: number
  viewCount: number
  releaseDate: string
  genres: string[]
}

export interface DashboardIssue {
  title: string
  description: string
  link?: string
}

export interface DashboardImage {
  id?: number
  fileName?: string
  url?: string
}

export interface Movie {
  id: string
  title: string
  year: number
  genres: string[]
  status: "active" | "inactive" | "draft"
  rating: number
  poster: string
  type: "single" | "series"
  description: string
  director: string
  cast: string[]
  backdrop: string
  releaseDate: Date
  images: MovieImage[]
  videos: MovieVideo[]
  subtitles: MovieSubtitle[]
  numberOfSeasons?: number
  numberOfEpisodes?: number
}

export interface MovieImage {
  id: string
  type: "poster" | "backdrop"
  url: string
  uploadedAt: Date
}

export interface MovieVideo {
  id: string
  title: string
  type: "trailer" | "episode"
  url: string
  duration: number
  season?: number
  episode?: number
  uploadedAt: Date
}

export interface MovieSubtitle {
  id: string
  language: string
  code: string
  url: string
  uploadedAt: Date
}

export interface Genre {
  id: string
  name: string
  description: string
}

export interface User {
  id: string
  name: string
  email: string
  role: "admin" | "user"
  status: "active" | "inactive"
}

export interface Order {
  id: string
  userId: string
  plan: string
  amount: number
  status: "paid" | "pending" | "failed"
  date: Date
}

export interface AuditLog {
  id: string
  action: string
  userId: string
  timestamp: Date
  details: string
}
