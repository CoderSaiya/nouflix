import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {Observable} from 'rxjs';
import {Genre, Movie} from '../../models/movie.model';
import {GlobalResponse} from '../../models/api-response.model';
import {map} from 'rxjs/operators';

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
            .sort((a, b) => a.name.localeCompare(b.name, 'vi', { sensitivity: 'accent' }))
        )
      )
  }
}
