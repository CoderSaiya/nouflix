import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import {environment} from '../../../environments/environment';
import {GlobalResponse} from '../../models/api-response.model';
import {VideoAsset} from '../../models/movie.model';

@Injectable({ providedIn: 'root' })
export class VideoService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/api/video-assets`;

  getByMovie(movieId: number): Observable<VideoAsset[]> {
    return this.http.get<GlobalResponse<VideoAsset[]>>(`${this.base}/movie/${movieId}`).pipe(map(r => r.data ?? []));
  }

  getByEpisode(episodeId: number): Observable<VideoAsset[]> {
    return this.http.get<GlobalResponse<VideoAsset[]>>(`${this.base}/episode/${episodeId}`).pipe(map(r => r.data ?? []));
  }

  getByEpisodeIds(ids: number[]): Observable<VideoAsset[]> {
    const fd = new FormData();
    ids.forEach(i => fd.append('ids', String(i)));
    return this.http.post<GlobalResponse<VideoAsset[]>>(`${this.base}/by-episodes`, fd).pipe(map(r => r.data ?? []));
  }

  delete(id: number) {
    return this.http.delete(`${this.base}/${id}`);
  }
}
