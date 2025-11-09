import {inject, Injectable} from "@angular/core"
import { type Observable, of } from "rxjs"
import {delay, map} from "rxjs/operators"
import {AdminDashboardStats, AuditLog, Genre, Movie, Order, User} from '../../models/admin.model';
import {GlobalResponse} from '../../models/api-response.model';
import {environment} from '../../../environments/environment';
import {HttpClient} from '@angular/common/http';

@Injectable({
  providedIn: "root",
})
export class AdminService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/dashboard`

  // Dashboard
  getDashboardStats(): Observable<AdminDashboardStats> {
    return this.http.get<GlobalResponse<AdminDashboardStats>>(`${this.apiUrl}`)
      .pipe(map(r => r.data));
  }

  // Movies
  getMovies(): Observable<Movie[]> {
    const mockMovies: Movie[] = [
      {
        id: "1",
        title: "Hành Trình Vượt Thời Gian",
        year: 2024,
        genres: ["Sci-Fi", "Drama"],
        status: "active",
        rating: 8.5,
        poster: "/placeholder.svg?height=300&width=200",
        type: "single",
        description: "",
        director: "",
        cast: [],
        backdrop: "",
        releaseDate: new Date(),
        images: [],
        videos: [],
        subtitles: []
      },
      {
        id: "2",
        title: "Tình Yêu Nơi Thành Phố",
        year: 2023,
        genres: ["Romance", "Comedy"],
        status: "active",
        rating: 7.2,
        poster: "/placeholder.svg?height=300&width=200",
        type: "single",
        description: "",
        director: "",
        cast: [],
        backdrop: "",
        releaseDate: new Date(),
        images: [],
        videos: [],
        subtitles: []
      },
    ]
    return of(mockMovies).pipe(delay(500))
  }

  // Genres
  getGenres(): Observable<Genre[]> {
    const mockGenres: Genre[] = [
      { id: "1", name: "Hành Động", description: "Phim hành động" },
      { id: "2", name: "Tình Cảm", description: "Phim tình cảm" },
      { id: "3", name: "Khoa Học Viễn Tưởng", description: "Phim sci-fi" },
    ]
    return of(mockGenres).pipe(delay(500))
  }

  // Users
  getUsers(): Observable<User[]> {
    const mockUsers: User[] = [
      {
        id: "1",
        name: "Nguyễn Văn A",
        email: "nguyenvana@email.com",
        role: "user",
        status: "active",
      },
      {
        id: "2",
        name: "Trần Thị B",
        email: "tranthib@email.com",
        role: "user",
        status: "active",
      },
    ]
    return of(mockUsers).pipe(delay(500))
  }

  // Orders
  getOrders(): Observable<Order[]> {
    const mockOrders: Order[] = [
      {
        id: "1",
        userId: "1",
        plan: "Premium",
        amount: 99000,
        status: "paid",
        date: new Date(),
      },
      {
        id: "2",
        userId: "2",
        plan: "Standard",
        amount: 59000,
        status: "pending",
        date: new Date(),
      },
    ]
    return of(mockOrders).pipe(delay(500))
  }

  // Audit Logs
  getAuditLogs(): Observable<AuditLog[]> {
    const mockLogs: AuditLog[] = [
      {
        id: "1",
        action: "Tạo phim mới",
        userId: "admin",
        timestamp: new Date(),
        details: 'Tạo phim "Hành Trình Vượt Thời Gian"',
      },
      {
        id: "2",
        action: "Cập nhật người dùng",
        userId: "admin",
        timestamp: new Date(Date.now() - 3600000),
        details: "Cập nhật thông tin người dùng ID: 1",
      },
    ]
    return of(mockLogs).pipe(delay(500))
  }
}
