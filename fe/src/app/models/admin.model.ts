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
  timestampUtc: string
  correlationId: string
  action: string
  userId: string
  username: string
  resourceType: string
  resourceId: string
  details: string
  clientIp: string
  userAgent: string
  route: string
  httpMethod: string
  statusCode: number
}

export interface LogItem {
  _id: string
  '@timestamp': string
  level: string
  message: string
  audit?: AuditLog | null
}

export interface LogResponse {
  total: number
  page: number
  size: number
  items: LogItem[]
}
