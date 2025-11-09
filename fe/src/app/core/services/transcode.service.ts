import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer, switchMap, map, takeWhile, throwError, catchError } from 'rxjs';
import {environment} from '../../../environments/environment';

export interface TranscodeStatus {
  jobId: string;
  state: string; // Queued | Running | Done | Failed
  progress: number;
  error?: string;
  masterKey?: string | null;
}

@Injectable({ providedIn: 'root' })
export class TranscodeService {
  private apiUrl = `${environment.apiUrl}/api/transcode`

  constructor(private http: HttpClient) {}

  /**
   * Tải file + metadata, backend sẽ trả về jobId
   */
  uploadAndEnqueue(
    movieId: number,
    episodeId: number | null,
    episodeNumber: number | null,
    seasonId: number | null,
    seasonNumber: number | null,
    language: string,
    profiles: string[],
    file: File
  ): Observable<{ jobId: string }> {
    const form = new FormData();
    form.append('movieId', String(movieId));
    if (episodeId != null) form.append('episodeId', String(episodeId));
    if (episodeNumber != null) form.append('episodeNumber', String(episodeNumber));
    if (seasonId != null) form.append('seasonId', String(seasonId));
    if (seasonNumber != null) form.append('seasonNumber', String(seasonNumber));
    form.append('language', language || 'vi');
    for (const p of profiles) form.append('profiles', p);
    form.append('file', file, file.name);

    return this.http.post<{ jobId: string }>(`${this.apiUrl}/upload`, form);
  }

  /**
   * Lấy status lần đầu
   */
  getStatus(jobId: string): Observable<TranscodeStatus> {
    return this.http.get<TranscodeStatus>(`${this.apiUrl}/${jobId}/status`);
  }

  /**
   * poll status đến khi xong task
   * intervalMs mặc định 2000.
   */
  pollStatus(jobId: string, intervalMs = 2000): Observable<TranscodeStatus> {
    return timer(0, intervalMs).pipe(
      switchMap(() => this.getStatus(jobId)),
      // emit until state is Done, but include final Done item
      takeWhile(s => !/^done$/i.test(s.state) && !/^failed$/i.test(s.state), true),
      catchError(err => throwError(() => err))
    );
  }
}
