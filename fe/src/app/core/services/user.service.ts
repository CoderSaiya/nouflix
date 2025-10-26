import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {AuthService} from './auth.service';
import {Observable} from 'rxjs';

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
}
