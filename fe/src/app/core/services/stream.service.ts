import {inject, Injectable} from '@angular/core';
import {environment} from '../../../environments/environment';
import {HttpClient} from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class StreamService {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/stream`;

  movieMasterUrl(movieId: number): string {
    return `${this.base}/movies/${movieId}/master.m3u8`;
  }

  episodeMasterUrl(movieId: number, episodeId: number): string {
    return `${this.base}/movies/${movieId}/episodes/${episodeId}/master.m3u8`;
  }

  fetchText(url: string) {
    return this.http.get(url, { responseType: 'text' });
  }
}

function joinUrl(...parts: string[]) {
  return parts
    .filter(Boolean)
    .map((p, i) => (i === 0 ? p.replace(/\/+$/g, '') : p.replace(/^\/+|\/+$/g, '')))
    .join('/');
}
