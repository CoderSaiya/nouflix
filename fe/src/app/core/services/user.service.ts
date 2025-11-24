import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
import { User, WatchHistoryItem, WatchlistItem } from '../../models/user.model';
import { map } from 'rxjs/operators';
import { GlobalResponse, SearchResponse } from '../../models/api-response.model';

@Injectable({
  providedIn: "root",
})
export class UserService {
  private http = inject(HttpClient)
  private auth = inject(AuthService);

  private apiUrl = `${environment.apiUrl}/api/user`

  progress(movieId: number, episodeId: number | null, position: number): Observable<void> {
    const userId = this.auth.currentUser()?.userId;
    if (!userId) {
      return new Observable<void>((sub) => {
        sub.complete();
      });
    }

    const body = {
      userId,
      movieId,
      episodeId,
      position,
    };

    return this.http.post<void>(
      `${this.apiUrl}/history/progress`, body
    );
  }

  getWatchHistory(): Observable<WatchHistoryItem[]> {
    return this.http
      .get<GlobalResponse<WatchHistoryItem[]>>(`${this.apiUrl}/history`)
      .pipe(
        map(res =>
          (res.data || undefined)
        )
      );
  }

  getWatchlist(): Observable<WatchlistItem[]> {
    return this.http.get<GlobalResponse<WatchlistItem[]>>(`${this.apiUrl}/watch-list`)
      .pipe(map(res => res.data ?? []));
  }

  getUsers(q?: string, page: number = 1, size: number = 10): Observable<User[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());

    if (q && q.trim().length > 0) {
      params = params.set('q', q.trim());
    }

    return this.http.get<GlobalResponse<SearchResponse<User[]>>>(`${this.apiUrl}`, { params })
      .pipe(map(res => res.data?.data ?? []));
  }
}
