import { inject, Injectable } from "@angular/core"
import { type Observable, of } from "rxjs"
import { delay, map } from "rxjs/operators"
import { AdminDashboardStats, AuditLog, Genre, LogResponse, Order, User } from '../../models/admin.model';
import { GlobalResponse } from '../../models/api-response.model';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: "root",
})
export class AdminService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api`

  // Dashboard
  getDashboardStats(): Observable<AdminDashboardStats> {
    return this.http.get<GlobalResponse<AdminDashboardStats>>(`${this.apiUrl}/dashboard`)
      .pipe(map(r => r.data));
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
  getAuditLogs(q?: string, page: number = 1, size: number = 25): Observable<AuditLog[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());

    if (q && q.trim().length > 0) {
      params = params.set('q', q.trim());
    }

    return this.http
      .get<LogResponse>(`${this.apiUrl}/log`, { params })
      .pipe(
        map(r => r.items),
        map(items =>
          items
            .map(i => i.audit)
            .filter((a): a is AuditLog => !!a)
        )
      );
  }
}
