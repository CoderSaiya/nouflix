import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Genre, Movie, Studio } from '../../models/movie.model';
import { GlobalResponse } from '../../models/api-response.model';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: "root",
})
export class TaxonomyService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/taxonomy`

  getGenres(q?: string | null): Observable<Genre[]> {
    return this.http
      .get<GlobalResponse<Genre[]>>(`${this.apiUrl}/genre/search`, {
        params: {
          q: q ?? ""
        }
      })
      .pipe(
        map(res =>
          (res.data || [])
            .slice()
            .sort((a, b) => a.id)
        )
      )
  }

  getGenreById(id: number): Observable<Genre | null> {
    return this.http.get<GlobalResponse<Genre | null>>(`${this.apiUrl}/genre/${id}`)
      .pipe(map(res => (res?.data || null)))
  }

  createGenre(payload: Partial<Genre>): Observable<Genre | null> {
    return this.http.post<GlobalResponse<Genre | null>>(`${this.apiUrl}/genre`, payload)
      .pipe(map(res => res?.data || null));
  }

  updateGenre(id: number, payload: Partial<Genre>): Observable<Genre | null> {
    return this.http.put<GlobalResponse<Genre | null>>(`${this.apiUrl}/genre/${id}`, payload)
      .pipe(map(res => res?.data || null));
  }

  getStudios(q?: string | null): Observable<Studio[]> {
    return this.http
      .get<GlobalResponse<Studio[]>>(`${this.apiUrl}/studio/search`, {
        params: { q: q ?? "" }
      })
      .pipe(map(res => res.data || []));
  }

  getStudioById(id: number): Observable<Studio | null> {
    return this.http.get<GlobalResponse<Studio | null>>(`${this.apiUrl}/studio/${id}`)
      .pipe(map(res => (res?.data || null)))
  }
}
