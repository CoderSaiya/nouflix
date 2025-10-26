import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  ViewChild,
  ElementRef, inject,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import type { Movie } from "../../models/movie.model";
import Hls from "hls.js";
import {AuthService} from '../../core/services/auth.service';

@Component({
  selector: "app-video-player",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./video-player.component.html",
  styleUrl: "./video-player.component.scss",
})
export class VideoPlayerComponent implements OnChanges, OnDestroy {
  @Input() movie!: Movie;
  @Input() isTheaterMode = false;
  /** URL master.m3u8: phim lẻ => /movies/{id}/master.m3u8; series => /movies/{id}/episodes/{episodeId}/master.m3u8 */
  @Input() masterUrl: string | null = null;
  /** URL subtitle: phim lẻ => /movies/{id}/sub/vi/index.vtt; series => /movies/{id}/episodes/{episodeId}/sub/vi/index.vtt */
  @Input() subtitleUrl: string | null = null;

  @Output() theaterModeToggle = new EventEmitter<void>();
  @Output() movieEnd = new EventEmitter<void>();

  @ViewChild("videoElement", { static: true })
  videoElement!: ElementRef<HTMLVideoElement>;

  private auth = inject(AuthService)

  private hls?: Hls;
  private controlsTimeout: any;

  // state UI
  isPlaying = false;
  isMuted = false;
  currentTime = 0;
  duration = 0;
  volume = 1;
  showControls = true;
  isFullscreen = false;
  playbackRate = 1;
  showSettings = false;
  showQuality = false;
  buffered = 0;

  // chất lượng (map từ HLS levels)
  qualities: string[] = ["Auto"];
  levels: { index: number; height?: number; bitrate?: number }[] = [];
  selectedLevelIndex = -1; // -1 = Auto
  selectedQuality = "Auto";

  // tốc độ
  playbackRates = [0.5, 1, 1.25, 1.5, 2];

  // phụ đề
  subtitleLang  = 'vi';
  subtitleLabel = 'Tiếng Việt';
  subtitleEnabled = true;

  // lifecycle
  ngOnChanges(changes: SimpleChanges): void {
    if (changes["masterUrl"]) this.load();
  }

  ngOnDestroy(): void {
    if (this.controlsTimeout) clearTimeout(this.controlsTimeout);
    this.destroyHls();
  }

  // HLS wiring
  private destroyHls() {
    try { this.hls?.destroy(); } catch {}
    this.hls = undefined;
    const v = this.videoElement?.nativeElement;
    if (v) v.pause();
  }

  private load() {
    const video = this.videoElement.nativeElement;

    // reset UI
    this.destroyHls();
    this.isPlaying = false;
    this.currentTime = 0;
    this.duration = 0;
    this.buffered = 0;
    this.selectedLevelIndex = -1;
    this.selectedQuality = "Auto";
    this.qualities = ["Auto"];
    this.levels = [{ index: -1 }];

    if (!this.masterUrl) {
      video.removeAttribute("src");
      return;
    }

    this.attachVideoEvents(video);

    const token = this.auth.token();

    // Safari HLS native
    if (video.canPlayType("application/vnd.apple.mpegurl")) {
      video.src = this.masterUrl;
      video.playbackRate = this.playbackRate;
      video.volume = this.volume;
      video.muted = this.isMuted;
      video.play().catch(() => {});
      return;
    }

    // hls.js cho browser khác
    if (Hls.isSupported()) {
      this.hls = new Hls({
        lowLatencyMode: true,
        backBufferLength: 30,
        fetchSetup: (ctx, initParams) => {
          const headers = new Headers(initParams?.headers || {});

          if (token) {
            console.log(token)
            headers.set("Authorization", `Bearer ${token}`);
          }

          return new Request(ctx.url, {
            ...initParams,
            headers,
            credentials: "include",
          });
        },
        xhrSetup: (xhr, url) => {
          // Cho phép gửi cookie
          xhr.withCredentials = true;

          if (token) {
            xhr.setRequestHeader("Authorization", `Bearer ${token}`);
          }
        },
      });
      this.hls.attachMedia(video);
      this.hls.loadSource(this.masterUrl);

      this.hls.on(Hls.Events.MANIFEST_PARSED, (_, data: any) => {
        const lvls = (data?.levels ?? []) as Array<{ height?: number; bitrate?: number }>;
        this.levels = [{ index: -1 }, ...lvls.map((l, i) => ({ index: i, height: l.height, bitrate: l.bitrate }))];
        this.qualities = this.levels.map(l =>
          l.index === -1 ? "Auto" : (l.height && l.height >= 2160 ? "4K" : `${l.height ?? Math.round((l.bitrate ?? 0)/1000)}p`)
        );
        this.selectedLevelIndex = -1;
        this.selectedQuality = "Auto";

        video.playbackRate = this.playbackRate;
        video.volume = this.volume;
        video.muted = this.isMuted;
        video.play().catch(() => {});
      });

      this.hls.on(Hls.Events.ERROR, (_, err) => {
        if (!this.hls) return;
        if (err.fatal) {
          switch (err.type) {
            case Hls.ErrorTypes.NETWORK_ERROR:
              this.hls.startLoad(); break;
            case Hls.ErrorTypes.MEDIA_ERROR:
              this.hls.recoverMediaError(); break;
            default:
              this.destroyHls();
          }
        }
      });
    }
  }

  private attachVideoEvents(video: HTMLVideoElement) {
    video.onloadedmetadata = () => {
      this.duration = isFinite(video.duration) ? video.duration : 0;
      this.updateBuffered(video);
      this.ensureSubtitleDisplay(video);
    };
    video.ontimeupdate = () => {
      this.currentTime = video.currentTime || 0;
      this.updateBuffered(video);
    };
    video.onprogress = () => this.updateBuffered(video);
    video.onplay = () => {
      this.isPlaying = true;
      this.onMouseMove(); // auto-hide timer
      this.ensureSubtitleDisplay(video);
    };
    video.onpause = () => {
      this.isPlaying = false;
      this.showControls = true;
    };
    video.onended = () => this.movieEnd.emit();
    video.onvolumechange = () => {
      this.isMuted = video.muted || video.volume === 0;
      this.volume = video.volume;
    };
    try {
      video.textTracks.addEventListener('addtrack', () => this.ensureSubtitleDisplay(video));
    } catch {}
  }

  private updateBuffered(video: HTMLVideoElement) {
    try {
      if (!video.buffered || this.duration <= 0) { this.buffered = 0; return; }
      const n = video.buffered.length;
      if (n === 0) { this.buffered = 0; return; }
      const end = video.buffered.end(n - 1);
      const percent = Math.max(0, Math.min(100, (end / this.duration) * 100));
      this.buffered = isFinite(percent) ? percent : 0;
    } catch { this.buffered = 0; }
  }

  // ===== UI handlers (giữ API cũ)
  onMouseMove(): void {
    this.showControls = true;
    if (this.controlsTimeout) clearTimeout(this.controlsTimeout);
    this.controlsTimeout = setTimeout(() => {
      if (this.isPlaying) this.showControls = false;
    }, 3000);
  }

  togglePlay(): void {
    const v = this.videoElement.nativeElement;
    if (v.paused || v.ended) v.play().catch(() => {}); else v.pause();
  }

  toggleMute(): void {
    const v = this.videoElement.nativeElement;
    v.muted = !v.muted;
    this.isMuted = v.muted;
  }

  toggleTheaterMode(): void { this.theaterModeToggle.emit(); }

  toggleFullscreen(): void {
    this.isFullscreen = !this.isFullscreen;
    const el = this.videoElement.nativeElement;
    if (this.isFullscreen) el.requestFullscreen?.();
    else if (document.fullscreenElement) document.exitFullscreen();
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
    if (!this.showSettings) this.showQuality = false;
  }

  toggleQuality(): void {
    if (this.levels.length <= 1) return;
    this.showQuality = !this.showQuality;
  }

  selectQuality(quality: string): void {
    if (!this.hls) {
      // Safari/native chỉ Auto
      this.selectedLevelIndex = -1;
      this.selectedQuality = "Auto";
      this.showQuality = this.showSettings = false;
      return;
    }
    if (quality === "Auto") {
      this.hls.currentLevel = -1;
      this.selectedLevelIndex = -1;
      this.selectedQuality = "Auto";
    } else {
      let idx = -1;
      if (quality === "4K") {
        const found = [...this.levels].reverse().find((l) => (l.height ?? 0) >= 2160);
        idx = found?.index ?? -1;
      } else {
        const m = quality.match(/(\d+)\s*p/i);
        if (m) {
          const h = +m[1];
          const found = this.levels.find((l) => l.height === h);
          idx = found?.index ?? -1;
        }
      }
      this.hls.currentLevel = idx;
      this.selectedLevelIndex = idx;
      this.selectedQuality = quality;
    }
    this.showQuality = false;
    this.showSettings = false;
  }

  selectPlaybackRate(rate: number): void {
    this.playbackRate = rate;
    const v = this.videoElement.nativeElement;
    v.playbackRate = rate;
    this.showSettings = false;
  }

  onProgressClick(event: MouseEvent): void {
    const v = this.videoElement.nativeElement;
    const progressBar = event.currentTarget as HTMLElement;
    const rect = progressBar.getBoundingClientRect();
    const percent = (event.clientX - rect.left) / rect.width;
    v.currentTime = Math.max(0, Math.min(1, percent)) * (this.duration || v.duration || 0);
  }

  onVolumeChange(event: Event): void {
    const v = this.videoElement.nativeElement;
    const input = event.target as HTMLInputElement;
    const val = Number(input.value);
    v.volume = Math.max(0, Math.min(1, val));
    v.muted = v.volume === 0;
    this.isMuted = v.muted;
    this.volume = v.volume;
  }

  skipForward(): void {
    const v = this.videoElement.nativeElement;
    v.currentTime = Math.min(v.currentTime + 10, this.duration || v.duration || 0);
  }

  skipBackward(): void {
    const v = this.videoElement.nativeElement;
    v.currentTime = Math.max(v.currentTime - 10, 0);
  }

  formatTime(seconds: number): string {
    const s = Math.floor(seconds || 0);
    const h = Math.floor(s / 3600);
    const m = Math.floor((s % 3600) / 60);
    const r = s % 60;
    return h > 0
      ? `${h}:${m.toString().padStart(2, "0")}:${r.toString().padStart(2, "0")}`
      : `${m}:${r.toString().padStart(2, "0")}`;
  }

  ensureSubtitleDisplay(video: HTMLVideoElement) {
    for (let i = 0; i < video.textTracks.length; i++) {
      const t = video.textTracks[i];
      if (t.kind === 'subtitles') t.mode = this.subtitleEnabled ? 'showing' : 'hidden';
    }
  }

  toggleSubtitles(): void {
    this.subtitleEnabled = !this.subtitleEnabled;
    this.ensureSubtitleDisplay(this.videoElement.nativeElement);
  }

  get progressPercent(): number {
    const v = this.videoElement?.nativeElement;
    const dur = this.duration || v?.duration || 0;
    const cur = this.currentTime || v?.currentTime || 0;
    return dur > 0 ? (cur / dur) * 100 : 0;
  }

  get volumeIcon(): string {
    const v = this.videoElement?.nativeElement;
    const vol = v?.volume ?? this.volume;
    if (v?.muted || vol === 0) return "🔇";
    if (vol < 0.5) return "🔉";
    return "🔊";
  }
}
