import { Component, inject, OnInit } from "@angular/core"
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms"
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog"
import { MovieService } from '../../../core/services/movie.service';
import { Episode, Genre, Movie, MovieType, PublishStatus, Season, Studio, VideoAsset, Subtitle } from '../../../models/movie.model';
import { TranscodeService, TranscodeStatus } from '../../../core/services/transcode.service';
import { VideoService } from '../../../core/services/video.service';
import { SubtitleService } from '../../../core/services/subtitle.service';
import { AdminService } from '../../../core/services/admin.service';
import { firstValueFrom } from 'rxjs';
import { TaxonomyService } from '../../../core/services/taxonomy.service';
import { toISODateString } from '../../../lib/utils/date.util';
import { MovieInfoComponent } from './info/movie-info.component';
import { MovieImagesComponent } from './images/movie-images.component';
import { MovieVideosComponent } from './videos/movie-videos.component';
import { MovieSubtitlesComponent } from './subtitles/movie-subtitles.component';
import { MatSnackBar } from "@angular/material/snack-bar";

@Component({
  selector: "app-movie-modal",
  templateUrl: "./movie-modal.component.html",
  styleUrls: ["./movie-modal.component.scss"],
  imports: [
    ReactiveFormsModule,
    MovieInfoComponent,
    MovieImagesComponent,
    MovieVideosComponent,
    MovieSubtitlesComponent
  ]
})
export class MovieModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  public dialogRef = inject(MatDialogRef<MovieModalComponent>);
  private data = inject(MAT_DIALOG_DATA) as { movieId?: string };
  private movieService = inject(MovieService);
  private transcode = inject(TranscodeService);
  private video = inject(VideoService)
  private taxo = inject(TaxonomyService)
  private subtitleService = inject(SubtitleService);
  private adminService = inject(AdminService);
  private snack = inject(MatSnackBar);

  activeTab: "info" | "images" | "videos" | "subtitles" = "info"
  isEditMode = false
  movie: Movie | null = null
  videos: VideoAsset[] = [];
  subtitles: Subtitle[] = [];
  genres: Genre[] = [];
  studios: Studio[] = [];
  seasons: Season[] = [];
  epsBySeason: Record<number, Episode[]> = {};
  videosByEpisode: Record<number, VideoAsset[]> = {};
  isBusy = false;
  busyText = '';
  error: Error | undefined = undefined;

  infoForm!: FormGroup
  imagesForm!: FormGroup
  videosForm!: FormGroup
  subtitlesForm!: FormGroup

  selectedPosters: File[] = []
  selectedBackdrops: File[] = []
  selectedVideos: File[] = [];

  readonly movieTypeLabels: Record<MovieType, string> = {
    [MovieType.Single]: 'Phim Lẻ',
    [MovieType.Series]: 'Phim Bộ',
  };

  readonly publishStatusLabels: Record<PublishStatus, string> = {
    [PublishStatus.Draft]: 'Bản Nháp',
    [PublishStatus.Published]: 'Đã Xuất Bản',
    [PublishStatus.Hidden]: 'Ẩn',
    [PublishStatus.InReview]: 'Đang Duyệt',
  };

  get movieTypeEntries() {
    return Object.entries(this.movieTypeLabels) as [string, string][];
  }

  get publishStatusEntries() {
    return Object.entries(this.publishStatusLabels) as [string, string][];
  }

  ngOnInit(): void {
    this.isEditMode = !!this.data?.movieId;

    this.initForms()
    this.loadReferenceData();
    if (this.isEditMode && this.data?.movieId) {
      this.loadMovieData()
    }
  }

  initForms(): void {
    this.infoForm = this.fb.group({
      title: ["", [Validators.required, Validators.minLength(3)]],
      type: [MovieType.Single, Validators.required],
      year: [new Date().getFullYear(), Validators.required],
      releaseDate: ["", Validators.required],
      genres: [[]],
      studioIds: [[]],
      description: ["", Validators.required],
      status: [PublishStatus.Draft, Validators.required],
      rating: [0, [Validators.required, Validators.min(0), Validators.max(10)]],
      director: [""],
      cast: [""],
      numberOfSeasons: [1],
      numberOfEpisodes: [1],
    })

    this.imagesForm = this.fb.group({
      posters: [[]],
      backdrops: [[]],
    })

    this.videosForm = this.fb.group({
      trailer: [""],
      episodes: [[]],
    })

    this.subtitlesForm = this.fb.group({
      subtitles: [[]],
    })
  }

  async loadMovieData(): Promise<void> {
    const idRaw = this.data?.movieId;
    const id = Number(idRaw);

    // đảm bảo id hợp lệ
    if (!idRaw || isNaN(id) || id <= 0) {
      console.warn('No valid movieId provided → skip load', idRaw);
      return;
    }

    try {
      const movie = await firstValueFrom(this.movieService.getById(id));
      if (!movie) {
        console.warn(`Movie with id=${id} not found`);
        return;
      }

      this.movie = movie;
      const genreIds = (movie.genres || []).map((g: any) => g.id);
      const studioIds = (movie.studios || []).map((s: any) => s.id);

      // Load videos
      try {
        this.videos = await firstValueFrom(this.video.getByMovie(id));
      } catch (errVideos) {
        console.error('Failed to load videos for movie', errVideos);
        this.videos = [];
      }

      // Load subtitles
      try {
        this.subtitles = await firstValueFrom(this.subtitleService.getByMovie(id));
      } catch (errSubs) {
        console.error('Failed to load subtitles', errSubs);
        this.subtitles = [];
      }

      // Load seasons and episodes for Series
      if (movie.type === MovieType.Series) {
        try {
          // Load seasons
          this.seasons = await firstValueFrom(this.movieService.getSeasons(id));
          console.log('Loaded seasons:', this.seasons);

          // Load all episodes for this movie
          const allEpisodes = await firstValueFrom(this.movieService.getEpisodesByMovie(id));
          console.log('Loaded episodes:', allEpisodes);

          // Group episodes by seasonId
          this.epsBySeason = {};
          for (const ep of allEpisodes) {
            if (!ep.seasonId) continue;
            if (!this.epsBySeason[ep.seasonId]) {
              this.epsBySeason[ep.seasonId] = [];
            }
            this.epsBySeason[ep.seasonId].push(ep);
          }

          // Load videos for each episode
          for (const ep of allEpisodes) {
            if (ep.id) {
              try {
                const epVideos = await firstValueFrom(this.video.getByEpisode(ep.id));
                this.videosByEpisode[ep.id] = epVideos || [];
              } catch (errEpVid) {
                console.error(`Failed to load videos for episode ${ep.id}`, errEpVid);
                this.videosByEpisode[ep.id] = [];
              }
            }
          }
        } catch (errSeries) {
          console.error('Failed to load seasons/episodes', errSeries);
          this.seasons = [];
          this.epsBySeason = {};
          this.videosByEpisode = {};
        }
      }

      const rd = movie.releaseDate ? new Date(movie.releaseDate) : null;
      this.infoForm.patchValue({
        ...movie,
        releaseDate: rd ? rd.toISOString().slice(0, 10) : '',
        type: movie.type ?? MovieType.Single,
        status: movie.status ?? PublishStatus.Draft,
        genres: genreIds,
        studioIds: studioIds,
      });

    } catch (err) {
      console.error('Load movie failed', err);
    }
  }

  private loadReferenceData() {
    this.taxo.getGenres().subscribe({
      next: g => this.genres = g,
      error: e => { console.error('load genres failed', e); this.genres = [] }
    });

    this.taxo.getStudios?.().subscribe({
      next: s => this.studios = s,
      error: e => { console.error('load studios failed', e); this.studios = [] }
    });
  }


  switchTab(tab: "info" | "images" | "videos" | "subtitles"): void {
    this.activeTab = tab
    console.log(this.movie)
  }

  toggleGenreId(id: number) {
    const arr: number[] = this.infoForm.get('genres')?.value || [];
    const idx = arr.indexOf(id);
    if (idx >= 0) arr.splice(idx, 1);
    else arr.push(id);
    this.infoForm.patchValue({ genres: [...arr] });
  }

  toggleStudioId(id: number) {
    const arr: number[] = this.infoForm.get('studioIds')?.value || [];
    const idx = arr.indexOf(id);
    if (idx >= 0) arr.splice(idx, 1);
    else arr.push(id);
    this.infoForm.patchValue({ studioIds: [...arr] });
  }

  onPosterFilesSelected(files: File[]) {
    files.map(f => this.selectedPosters.push(f))
    this.imagesForm.patchValue({ posters: files });
  }

  onBackdropFilesSelected(files: File[]) {
    files.map(f => this.selectedBackdrops.push(f))
    this.imagesForm.patchValue({ backdrops: files });
  }

  onSubtitleFileSelected(files: FileList) {

  }

  removePoster(index: number): void {
    this.selectedPosters.splice(index, 1)
  }

  removeBackdrop(index: number): void {
    this.selectedBackdrops.splice(index, 1)
  }

  submit(): void {
    if (this.infoForm.valid) {
      const formValue = this.infoForm.value;
      const payload = {
        ...formValue,
        genres: formValue.genres.map((g: string) => ({ name: g })),
        type: Number(formValue.type),
        status: Number(formValue.status),
        genreIds: formValue.genres || [],
        studioIds: formValue.studioIds || []
      };
      console.log("[v0] Movie modal submitted:", payload);
      this.dialogRef.close(payload);
    }
  }

  cancel(): void {
    this.dialogRef.close()
  }

  private async busyRun<T>(text: string, work: () => Promise<T>): Promise<T> {
    this.error = undefined;
    this.busyText = text;
    this.isBusy = true;
    try {
      return await work();
    } catch (err) {
      // optional: display friendly error
      console.error(err);
      throw err;
    } finally {
      this.isBusy = false;
      this.busyText = '';
    }
  }

  async addSeason() {
    if (!this.movie || !this.movie.id) {
      alert('Phải lưu phim trước khi thêm mùa.');
      return;
    }
    try {
      const next = (this.seasons[this.seasons.length - 1]?.number ?? 0) + 1;
      // gọi API createSeason
      const s = await firstValueFrom(this.movieService.createSeason(this.movie.id, {
        number: next,
        title: `Season ${next}`,
        year: new Date().getFullYear()
      }));
      this.seasons.push(s);
      this.epsBySeason[s.id] = [];
    } catch (err) {
      console.error('Add season failed', err);
      alert('Tạo mùa thất bại');
    }
  }

  async onUploadMovieVideo(f: File) {
    if (!this.movie?.id) { alert('Phải lưu phim trước khi upload video.'); return; }
    const profiles = ['1080', '720', '480'];
    try {
      await this.busyRun('Đang xử lý video phim lẻ…', async () => {
        const resp = await firstValueFrom(this.transcode.uploadAndEnqueue(this.movie!.id, null, null, null, null, this.movie!.language ?? 'vi', profiles, f));
        await this.pollAndHandle(resp.jobId);
        await this.reloadMovieVideos();
      });
    } catch (err) { console.error(err); alert('Upload thất bại'); }
  }

  async onUploadEpisodeVideo(season: Season, episode: Episode, f: File) {
    if (!this.movie || !this.movie.id) {
      alert('Phải lưu phim trước khi upload video.');
      return;
    }

    // ensure episode exists on server
    if (!episode.id || episode.id === 0) {
      try {
        episode.id = await firstValueFrom(this.movieService.upsertEpisode({
          movieId: this.movie.id,
          seasonId: season.id,
          episodeNumber: episode.number,
          title: episode.title ?? `Tập ${episode.number}`,
          status: episode.status ?? PublishStatus.Published,
          releaseDate: episode.releaseDate ?? new Date()
        }));
      } catch (err) {
        console.error('Failed to save episode', err);
        alert('Không thể lưu tập trước khi upload');
        return;
      }
    }

    const profiles = ['1080', '720', '480'];

    try {
      await this.busyRun(`Đang xử lý tập ${episode.number}…`, async () => {
        const resp = await firstValueFrom(this.transcode.uploadAndEnqueue(
          this.movie!.id,
          episode.id,
          episode.number,
          season.id,
          season.number,
          (this.movie as any).language ?? 'vi',
          profiles,
          f
        ));
        const jobId = resp.jobId;
        await this.pollAndHandle(jobId, `Đang xử lý tập ${episode.number}…`);
        await this.reloadEpisodeVideos(episode.id);
      });
    } catch (err) {
      console.error('Upload episode failed', err);
      alert('Upload tập thất bại: ' + (err));
    }
  }

  async renameSeason(s: Season) {
    try {
      const updated = await firstValueFrom(this.movieService.updateSeason(s.id!, { title: `Season ${s.number}` }));
      const idx = this.seasons.findIndex(x => x.id === s.id);
      if (idx >= 0) this.seasons[idx] = updated;
    } catch (err) {
      console.error('Rename season failed', err);
    }
  }

  async deleteSeason(s: Season) {
    if (!confirm('Xoá mùa sẽ xoá tất cả tập & video liên quan. Tiếp tục?')) return;
    try {
      await firstValueFrom(this.movieService.deleteSeason(s.id!));
      this.seasons = this.seasons.filter(x => x.id !== s.id);
      const eps = this.epsBySeason[s.id!];
      if (eps) {
        for (const e of eps) {
          delete this.videosByEpisode[e.id!];
        }
      }
      delete this.epsBySeason[s.id!];
    } catch (err) {
      console.error('Delete season failed', err);
      alert('Xoá mùa thất bại');
    }
  }

  async saveEpisode(e: Episode) {
    if (!this.movie || !this.movie.id || !e.seasonId) {
      alert('Không thể lưu episode');
      return;
    }
    try {
      const req = {
        movieId: this.movie.id,
        seasonId: e.seasonId,
        episodeNumber: e.number,
        title: e.title,
        status: e.status,
        releaseDate: (e as any).releaseDateStr ? new Date((e as any).releaseDateStr) : e.releaseDate ?? new Date()
      };
      e.id = await firstValueFrom(this.movieService.upsertEpisode(req));

      if (!this.videosByEpisode[e.id]) this.videosByEpisode[e.id] = [];
    } catch (err) {
      console.error('Save episode failed', err);
      alert('Lưu tập thất bại');
    }
  }

  async deleteEpisode(season: Season, e: Episode) {
    if (!e.id || e.id === 0) {
      // chưa sync server -> chỉ xóa local
      const list = this.epsBySeason[season.id!] || [];
      this.epsBySeason[season.id!] = list.filter(x => x !== e);
      return;
    }
    try {
      await firstValueFrom(this.movieService.deleteEpisode(e.id));
      const list = this.epsBySeason[season.id!] || [];
      this.epsBySeason[season.id!] = list.filter(x => x.id !== e.id);
      delete this.videosByEpisode[e.id];
    } catch (err) {
      console.error('Delete episode failed', err);
    }
  }

  addEpisode(s: Season) {
    const list = this.epsBySeason[s.id!] ?? (this.epsBySeason[s.id!] = []);
    const next = (list[list.length - 1]?.number ?? 0) + 1;
    const e: Episode = {
      id: 0,
      seasonId: s.id,
      seasonNumber: s.number,
      number: next,
      title: `Tập ${next}`,
      overview: `Tập ${next}`,
      status: PublishStatus.Published,
      runtime: 0,
      releaseDate: (new Date()).toString()
    };

    (e as any).releaseDateStr = toISODateString(e.releaseDate);
    list.push(e);
  }

  // đăng ký pollStatus, cập nhật busyText khi tiến trình và giải quyết khi Hoàn tất
  private pollAndHandle(jobId: string, initialText = 'Đang xử lý…'): Promise<void> {
    return new Promise((resolve, reject) => {
      this.busyText = initialText;
      this.isBusy = true;

      const sub = this.transcode.pollStatus(jobId, 2000).subscribe({
        next: (st: TranscodeStatus) => {
          // cập nhật trạng thái/tiến độ cho UI
          if (/queued/i.test(st.state)) {
            this.busyText = 'Đang xếp hàng…';
          } else if (/running/i.test(st.state)) {
            // show numeric progress nếu backend trả progress
            const p = (!isNaN(st.progress)) ? `${st.progress}%` : '';
            this.busyText = p ? `Đang xử lý… ${p}` : 'Đang xử lý…';
          } else if (/done/i.test(st.state)) {
            this.busyText = 'Hoàn tất.';
          } else if (/failed/i.test(st.state)) {
            this.busyText = st.error ? `Lỗi: ${st.error}` : 'Xử lý thất bại';
          }
        },
        error: (err) => {
          sub.unsubscribe();
          this.isBusy = false;
          this.busyText = '';
          reject(err);
        },
        complete: () => {
          sub.unsubscribe();
          // giữ message "Hoàn tất." trong 1s rồi clear để người dùng thấy
          this.busyText = 'Hoàn tất.';
          setTimeout(() => { this.isBusy = false; this.busyText = ''; }, 800);
          resolve();
        }
      });
    });
  }

  private async reloadMovieVideos() {
    if (!this.movie || !this.movie.id) return;
    try {
      this.videos = await firstValueFrom(this.video.getByMovie(this.movie.id));
    } catch (err) {
      console.error('Failed reload movie videos', err);
    }
  }

  private async reloadEpisodeVideos(episodeId: number) {
    try {
      if (!episodeId) return;
      const vids = await firstValueFrom(this.video.getByEpisode(episodeId));

      this.videosByEpisode = this.videosByEpisode || {};
      this.videosByEpisode[episodeId] = vids || [];
    } catch (err) {
      console.error('Failed reload episode videos', err);
    }
  }

  goSubtitleForAsset(v: VideoAsset) {
    console.warn('goSubtitle not implemented in TS');
  }

  async removeVideoByAsset(id: number) {
    if (!confirm('Bạn có chắc muốn xoá video này?')) return;
    try {
      this.isBusy = true;
      await firstValueFrom(this.video.delete(id));
      this.videos = this.videos.filter(v => v.id !== id);
      // Also remove from episode videos if present
      for (const k of Object.keys(this.videosByEpisode || {})) {
        this.videosByEpisode[+k] = (this.videosByEpisode[+k] || []).filter(v => v.id !== id);
      }
      this.snack.open('Đã xoá video', 'Đóng', { duration: 2000 });
    } catch (err) {
      console.error(err);
      this.snack.open('Lỗi khi xoá video', 'Đóng', { duration: 3000 });
    } finally {
      this.isBusy = false;
    }
  }

  async removeSubtitle(id: number) {
    if (!confirm('Bạn có chắc muốn xoá phụ đề này?')) return;
    try {
      this.isBusy = true;
      await firstValueFrom(this.subtitleService.delete(id));
      this.subtitles = this.subtitles.filter(s => s.id !== id);
      this.snack.open('Đã xoá phụ đề', 'Đóng', { duration: 2000 });
    } catch (err) {
      console.error(err);
      this.snack.open('Lỗi khi xoá phụ đề', 'Đóng', { duration: 3000 });
    } finally {
      this.isBusy = false;
    }
  }

  protected readonly MovieType = MovieType;
}
