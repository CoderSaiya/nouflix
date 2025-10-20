import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {Observable} from 'rxjs';
import type {Episode, Season} from '../../models/movie.model';
import {GlobalResponse} from '../../models/api-response.model';
import {map} from 'rxjs/operators';

@Injectable({
  providedIn: "root",
})
export class EpisodeService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/episode`

  getSeasonEpisodes(seriesId: number, seasonNumber: number): Observable<Episode[]> {
    return this.http
      .get<GlobalResponse<Episode[]>>(`${this.apiUrl}/season/${seasonNumber}/movie/${seriesId}`)
      .pipe(
        map(res => (res.data || []))
      )
  }
}
