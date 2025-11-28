import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { GlobalResponse } from '../../models/api-response.model';
import { Subtitle } from '../../models/movie.model';

@Injectable({ providedIn: 'root' })
export class SubtitleService {
    private http = inject(HttpClient);
    private base = `${environment.apiUrl}/api/subtitle`;

    getByMovie(movieId: number): Observable<Subtitle[]> {
        return this.http.get<GlobalResponse<Subtitle[]>>(`${this.base}/movie/${movieId}`)
            .pipe(map(r => r.data ?? []));
    }

    getByEpisode(episodeId: number): Observable<Subtitle[]> {
        return this.http.get<GlobalResponse<Subtitle[]>>(`${this.base}/episode/${episodeId}`)
            .pipe(map(r => r.data ?? []));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<GlobalResponse<any>>(`${this.base}/${id}`)
            .pipe(map(() => void 0));
    }
}
