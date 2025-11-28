import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Episode, Movie, MovieType, Season, VideoAsset } from '../../../../models/movie.model';
import { FormGroup, FormsModule } from '@angular/forms';

@Component({
  selector: 'app-movie-videos',
  templateUrl: './movie-videos.component.html',
  imports: [
    FormsModule
  ],
  styleUrls: ['./movie-videos.component.scss']
})
export class MovieVideosComponent {
  @Input() infoForm!: FormGroup;
  @Input() movie!: Movie | null;
  @Input() videos: VideoAsset[] = [];
  @Input() seasons: Season[] = [];
  @Input() epsBySeason: Record<number, Episode[]> = {};
  @Input() videosByEpisode: Record<number, VideoAsset[]> = {};
  @Input() publishStatusEntries: [string, string][] = [];

  @Output() addSeason = new EventEmitter<void>();
  @Output() renameSeason = new EventEmitter<Season>();
  @Output() deleteSeason = new EventEmitter<Season>();
  @Output() addEpisode = new EventEmitter<Season>();
  @Output() saveEpisode = new EventEmitter<Episode>();
  @Output() deleteEpisode = new EventEmitter<{ season: Season, episode: Episode }>();
  @Output() uploadMovieVideo = new EventEmitter<File>();
  @Output() uploadEpisodeVideo = new EventEmitter<{ season: Season, episode: Episode, file: File }>();
  @Output() deleteAsset = new EventEmitter<number>();
  @Output() goSubtitle = new EventEmitter<VideoAsset>();

  get isSeries() { return this.infoForm?.get('type')?.value === MovieType.Series; }

  onMovieFileChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files?.[0]) return;
    this.uploadMovieVideo.emit(input.files[0]);
    input.value = '';
  }

  onEpisodeFileChange(s: Season, e: Episode, ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files?.[0]) return;
    this.uploadEpisodeVideo.emit({ season: s, episode: e, file: input.files[0] });
    input.value = '';
  }

  safeEpisodeNumber(e: Episode): number {
    return (e && true) ? e.number : 0;
  }

  onEpisodeNumberChange(e: Episode, ev: Event) {
    const input = ev.target as HTMLInputElement;
    const v = input.value;
    const parsed = Number(v);
    e.number = Number.isFinite(parsed) ? parsed : 0;
  }

  onEpisodeTitleChange(e: Episode, ev: Event) {
    const input = ev.target as HTMLInputElement;
    e.title = input.value;
  }

  onEpisodeDateChange(e: Episode, ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (input.value) {
      e.releaseDate = new Date(input.value).toISOString();
      (e as any).releaseDateStr = input.value;
    }
  }

  onEpisodeStatusChange(e: Episode, ev: Event) {
    const select = ev.target as HTMLSelectElement;
    e.status = Number(select.value);
  }

  getEpisodeDateValue(e: Episode): string {
    if (!e) return '';
    if (e.releaseDate) {
      const d = new Date(e.releaseDate);
      if (isNaN(d.getTime())) return '';
      return d.toISOString().slice(0, 10);
    }
    return '';
  }

  hasVideosInSeason(seasonId: number) {
    const eps = this.epsBySeason[seasonId] || [];
    return eps.some(ep => ep.id && this.videosByEpisode[ep.id] && this.videosByEpisode[ep.id].length > 0);
  }

  findEpisodeNumber(epId: number | undefined) {
    if (!epId) return '?';
    for (const sid of Object.keys(this.epsBySeason)) {
      const arr = this.epsBySeason[+sid] || [];
      const ep = arr.find(x => x.id === epId);
      if (ep) return ep.number;
    }
    return '?';
  }

  episodeIdsInSeason(seasonId: number) {
    const eps = this.epsBySeason[seasonId] || [];
    return eps.map(e => e.id).filter(id => id && id > 0);
  }

  protected readonly MovieType = MovieType;
}
