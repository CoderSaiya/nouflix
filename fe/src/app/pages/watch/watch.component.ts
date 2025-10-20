import {Component, inject, type OnDestroy, type OnInit} from "@angular/core"
import {CommonModule} from "@angular/common"
import {ActivatedRoute, Router, RouterLink} from "@angular/router"
import {Meta, Title} from "@angular/platform-browser"
import {Subject, takeUntil} from "rxjs"
import {MovieService} from "../../core/services/movie.service"
import {Episode, Movie, MovieType, Season} from "../../models/movie.model"
import {VideoPlayerComponent} from "../../components/video-player/video-player.component"
import {WatchInfoComponent} from "../../components/watch-info/watch-info.component"
import {RecommendationsComponent} from "../../components/recommendations/recommendations.component"
import {SeasonService} from '../../core/services/season.service';
import {StreamService} from '../../core/services/stream.service';
import {EpisodeService} from '../../core/services/episode.service';

@Component({
  selector: "app-watch",
  standalone: true,
  imports: [CommonModule, RouterLink, VideoPlayerComponent, WatchInfoComponent, RecommendationsComponent],
  templateUrl: "./watch.component.html",
  styleUrl: "./watch.component.scss",
})
export class WatchComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private movieSvc = inject(MovieService)
  private seasonSvc = inject(SeasonService)
  private episodeSvc = inject(EpisodeService)
  private titleSvc = inject(Title)
  private metaSvc = inject(Meta)
  private streamSvc = inject(StreamService)

  movie: Movie | undefined
  similarMovies: Movie[] = []
  isLoading = true
  isTheaterMode = false
  seasons: Season[] = []
  currentSeason = 1
  currentEpisode = 1
  episodes: Episode[] = []
  selectedEpisode: Episode | undefined
  masterUrl: string | null = null
  subtitleUrl: string | null = null

  private destroy$ = new Subject<void>()

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const slug = String(params["slug"])
      this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((queryParams) => {
        this.currentSeason = Number(queryParams["season"]) || 1
        this.currentEpisode = Number(queryParams["episode"]) || 1
        this.loadMovie(slug)
      })
    })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private loadMovie(slug: string): void {
    this.isLoading = true

    this.movieSvc
      .getDetail(slug)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (movie) => {
          if (movie) {
            this.movie = movie
            const id = movie.id
            this.updateMetaTags(movie)
            this.loadSimilarMovies(id)
            if (movie.type === MovieType.Series) {
              this.loadSeriesData(id)
            } else {
              this.masterUrl = this.streamSvc.movieMasterUrl(movie.id);
              this.subtitleUrl = this.streamSvc.movieSubtitleUrl(movie.id);
            }
          } else {
            this.router.navigate(["/"])
          }
          this.isLoading = false
        },
        error: () => {
          this.isLoading = false
          this.router.navigate(["/"])
        },
      })
  }

  private loadSeriesData(seriesId: number): void {
    this.seasonSvc
      .getSeriesSeasons(seriesId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (seasons) => {
          this.seasons = seasons
          this.loadEpisodes(seriesId, this.currentSeason)
        },
      })
  }

  protected loadEpisodes(seriesId: number, seasonNumber: number): void {
    this.episodeSvc
      .getSeasonEpisodes(seriesId, seasonNumber)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (episodes) => {
          this.episodes = episodes
          this.selectedEpisode = episodes.find((e) => e.number === this.currentEpisode)

          if (this.movie && this.movie.type == MovieType.Series && this.selectedEpisode) {
            this.currentEpisode = this.selectedEpisode.number;
            this.masterUrl = this.streamSvc.episodeMasterUrl(this.movie.id, this.selectedEpisode.id);
            this.subtitleUrl = this.streamSvc.episodeSubtitleUrl(this.movie.id, this.selectedEpisode.id);
          }
        },
      })
  }

  private loadSimilarMovies(movieId: number): void {
    this.movieSvc
      .getSimilarMovies(movieId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (movies) => {
          this.similarMovies = movies
        },
      })
  }

  private updateMetaTags(movie: Movie): void {
    const title = `Xem ${movie.title} - NouFlix`
    this.titleSvc.setTitle(title)

    this.metaSvc.updateTag({ name: "description", content: movie.overview })
    this.metaSvc.updateTag({ property: "og:title", content: title })
    this.metaSvc.updateTag({ property: "og:description", content: movie.overview })
    this.metaSvc.updateTag({ property: "og:image", content: movie.backdropUrl })
    this.metaSvc.updateTag({
      property: "og:type",
      content: movie.type === MovieType.Series ? "video.tv_show" : "video.single",
    })
  }

  selectEpisode(seasonNumber: number, episodeNumber: number): void {
    if (this.movie && this.movie.type === MovieType.Series) {
      this.currentSeason = seasonNumber
      this.currentEpisode = episodeNumber
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { season: seasonNumber, episode: episodeNumber },
        queryParamsHandling: "merge",
      })
      this.loadEpisodes(this.movie.id, seasonNumber)
    }
  }

  playNextEpisode(): void {
    if (this.movie && this.movie.type === MovieType.Series) {
      const nextEpisode = this.episodes.find((e) => e.number === this.currentEpisode + 1)
      if (nextEpisode) {
        this.selectEpisode(this.currentSeason, this.currentEpisode + 1)
      } else {
        // Try next season
        const nextSeason = this.seasons.find((s) => s.number === this.currentSeason + 1)
        if (nextSeason) {
          this.selectEpisode(this.currentSeason + 1, 1)
        }
      }
    }
  }

  toggleTheaterMode(): void {
    this.isTheaterMode = !this.isTheaterMode
  }

  onMovieEnd(): void {
    // Auto-play next similar movie or show recommendations
    if (this.movie?.type === MovieType.Series) {
      this.playNextEpisode()
    }
    else console.log("Movie ended")
  }

  protected readonly MovieType = MovieType;
}
