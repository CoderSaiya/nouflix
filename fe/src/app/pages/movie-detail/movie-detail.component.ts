import {Component, inject, type OnInit} from "@angular/core"
import {CommonModule} from "@angular/common"
import {ActivatedRoute, Router, RouterLink} from "@angular/router"
import {Meta, Title} from "@angular/platform-browser"
import {MovieService} from "../../core/services/movie.service"
import {Cast, Crew, Movie, MovieType, Review, Season, Video} from "../../models/movie.model"
import {SeasonService} from '../../core/services/season.service';

@Component({
  selector: "app-movie-detail",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./movie-detail.component.html",
  styleUrl: "./movie-detail.component.scss",
})
export class MovieDetailComponent implements OnInit {
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private movieSvc = inject(MovieService)
  private seasonSvc = inject(SeasonService)
  private titleSvc = inject(Title)
  private metaSvc = inject(Meta)

  movie: Movie | null = null
  cast: Cast[] = []
  crew: Crew[] = []
  reviews: Review[] = []
  videos: Video[] = []
  similarMovies: Movie[] = []
  seasons: Season[] = []
  loading = true
  activeTab: "overview" | "cast" | "reviews" | "videos" | "seasons" = "overview"

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const slug = String(params["slug"])
      this.loadMovieDetails(slug)
    })
  }

  loadMovieDetails(slug: string): void {
    this.loading = true

    this.movieSvc.getDetail(slug).subscribe((movie) => {
      if (!movie) {
        this.router.navigate(["/"])
        return
      }

      this.movie = movie
      this.updateMetaTags(movie)
      this.loading = false

      const id = movie.id;

      // Load additional data
      this.movieSvc.getMovieCast(id).subscribe((cast) => {
        this.cast = cast
      })

      this.movieSvc.getMovieCrew(id).subscribe((crew) => {
        this.crew = crew
      })

      this.movieSvc.getMovieReviews(id).subscribe((reviews) => {
        this.reviews = reviews
      })

      this.movieSvc.getMovieVideos(id).subscribe((videos) => {
        this.videos = videos
      })

      this.movieSvc.getSimilarMovies(id).subscribe((similar) => {
        this.similarMovies = similar
      })

      if (movie.type === MovieType.Series) {
        this.seasonSvc.getSeriesSeasons(id).subscribe((seasons) => {
          this.seasons = seasons
          console.log(seasons)
        })
      }
    })
  }

  updateMetaTags(movie: Movie): void {
    this.titleSvc.setTitle(`${movie.title} - NouFlix`)

    this.metaSvc.updateTag({
      name: "description",
      content: movie.overview,
    })

    this.metaSvc.updateTag({
      name: "keywords",
      content: `${movie.title}, ${movie.genres.map((g) => g.name).join(", ")}, phim, xem phim online`,
    })

    // Open Graph tags for social sharing
    this.metaSvc.updateTag({
      property: "og:title",
      content: movie.title,
    })

    this.metaSvc.updateTag({
      property: "og:description",
      content: movie.overview,
    })

    this.metaSvc.updateTag({
      property: "og:image",
      content: movie.posterUrl,
    })

    this.metaSvc.updateTag({
      property: "og:type",
      content: "video.movie",
    })
  }

  setActiveTab(tab: "overview" | "cast" | "reviews" | "videos" | "seasons"): void {
    this.activeTab = tab
  }

  formatRuntime(minutes: number): string {
    const hours = Math.floor(minutes / 60)
    const mins = minutes % 60
    return `${hours}h ${mins}m`
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
    }).format(amount)
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString("vi-VN", {
      year: "numeric",
      month: "long",
      day: "numeric",
    })
  }

  getYouTubeEmbedUrl(key: string): string {
    return `https://www.youtube.com/embed/${key}`
  }

  getDirector(): Crew | undefined {
    return this.crew.find((c) => c.job === "Director")
  }

  getProducers(): Crew[] {
    return this.crew.filter((c) => c.job === "Producer")
  }

  getComposer(): Crew | undefined {
    return this.crew.find((c) => c.job === "Original Music Composer")
  }

  getRatingStars(rating: number): number[] {
    const stars = Math.round(rating / 2)
    return Array(5)
      .fill(0)
      .map((_, i) => (i < stars ? 1 : 0))
  }

  protected readonly MovieType = MovieType;
}
