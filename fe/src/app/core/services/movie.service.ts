import {inject, Injectable} from "@angular/core"
import {Observable, of} from "rxjs"
import {map} from 'rxjs/operators';
import {Cast, Crew, Episode, Genre, Movie, MovieItem, MovieType, Review, Video} from "../../models/movie.model"
import {MOCK_CAST, MOCK_CREW, MOCK_MOVIES, MOCK_REVIEWS, MOCK_SEASONS, MOCK_VIDEOS} from "../../data/mock-data"
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {GlobalResponse, SearchResponse} from '../../models/api-response.model';

@Injectable({
  providedIn: "root",
})
export class MovieService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/movie`

  getAllMovies(): Observable<Movie[]> {
    return of(MOCK_MOVIES)
  }

  getMoviesOnly(): Observable<Movie[]> {
    return of(MOCK_MOVIES.filter((m) => m.type === MovieType.Single))
  }

  getSeriesOnly(): Observable<Movie[]> {
    return of(MOCK_MOVIES.filter((m) => m.type === MovieType.Series))
  }

  getDetail(slug: string): Observable<Movie | undefined> {
    return this.http
      .get<GlobalResponse<Movie>>(`${this.apiUrl}/${slug}`)
      .pipe(
        map(res =>
          (res.data || undefined)
        )
      )
  }

  getById(id: number): Observable<Movie | undefined> {
    return this.http
      .get<GlobalResponse<Movie>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(res =>
          (res.data || undefined)
        )
      )
  }

  getTrendingMovies(take: number = 12): Observable<Movie[]> {
    return this.http
      .get<GlobalResponse<Movie[]>>(`${this.apiUrl}/trending`, {
        params: {
          take: take
        }
      })
      .pipe(
        map(res =>
          (res.data || [])
            .slice()
            .sort(
              (a, b) =>
                b.popularity - a.popularity)
            .slice(0, take),
        )
      )
  }

  getPopularMovies(take: number = 12): Observable<MovieItem[]> {
    return this.http
      .get<GlobalResponse<MovieItem[]>>(`${this.apiUrl}/popular`, {
        params: {
          take: take
        }
      })
      .pipe(
        map(res =>
          (res.data || [])
            .slice()
            .sort(
              (a, b) =>
                b.avgRating - a.avgRating)
            .slice(0, take),
        )
      )
  }

  getTopRatedMovies(take: number = 12): Observable<MovieItem[]> {
    return this.http
      .get<GlobalResponse<MovieItem[]>>(`${this.apiUrl}/most-rating`, {
        params: {
          take: take
        }
      })
      .pipe(
        map(res =>
          (res.data || [])
            .slice()
            .filter((m) => m.avgRating >= 8)
            .slice(0, take),
        )
      )
  }

  getNewReleases(take: number = 12): Observable<MovieItem[]> {
    return this.http
        .get<GlobalResponse<MovieItem[]>>(`${this.apiUrl}/new`, {
          params: {
            take: take
          }
        })
        .pipe(
          map(res =>
            (res.data || [])
              .slice()
              .sort(
                (a, b) =>
                  new Date(b.releaseDate).getTime() - new Date(a.releaseDate).getTime()
              )
              .slice(0, take),
          )
        )
  }

  getMoviesByGenre(genreId: number): Observable<MovieItem[]> {
    // return of(MOCK_MOVIES.filter((m) => m.genres.some((g) => g.id === genreId)))
    return this.http
      .get<GlobalResponse<MovieItem[]>>(`${this.apiUrl}/genre/${genreId}`)
      .pipe(
        map(res =>
          (res.data || [])
            .slice()
            .sort(
              (a, b) =>
                new Date(b.releaseDate).getTime() - new Date(a.releaseDate).getTime()
            )
        )
      )
  }

  searchMovies(query?: string | null, skip?: number | null, take?: number | null ): Observable<Movie[]> {
    const lowerQuery = query?.toLowerCase() ?? ""

    return this.http
      .get<GlobalResponse<SearchResponse<Movie[]>>>(`${this.apiUrl}/search`, {
        params: {
          q: lowerQuery,
          skip: skip ?? 0,
          take: take ?? 20
        }
      })
      .pipe(
        map(res =>
          (res.data.data || [])
            .slice()
            .sort((a, b) => a.title.localeCompare(b.title, 'vi', { sensitivity: 'accent' }))
        )
      )
  }

  getSimilarMovies(movieId: number): Observable<Movie[]> {
    const movie = MOCK_MOVIES.find((m) => m.id === movieId)
    if (!movie) return of([])

    const similarMovies = MOCK_MOVIES.filter(
      (m) => m.id !== movieId && m.genres.some((g) => movie.genres.some((mg) => mg.id === g.id)),
    ).slice(0, 6)

    return of(similarMovies)
  }

  getMovieCast(movieId: number): Observable<Cast[]> {
    return of(MOCK_CAST)
  }

  getMovieCrew(movieId: number): Observable<Crew[]> {
    return of(MOCK_CREW)
  }

  getMovieReviews(movieId: number): Observable<Review[]> {
    return of(MOCK_REVIEWS)
  }

  getMovieVideos(movieId: number): Observable<Video[]> {
    return of(MOCK_VIDEOS)
  }

  // getSeasonEpisodes(seriesId: number, seasonNumber: number): Observable<Episode[]> {
  //   const seasons = MOCK_SEASONS[seriesId] || []
  //   const season = seasons.find((s) => s.number === seasonNumber)
  //   return of(season?.episodes || [])
  // }

  getEpisode(seriesId: number, seasonNumber: number, episodeNumber: number): Observable<Episode | undefined> {
    const seasons = MOCK_SEASONS[seriesId] || []
    const season = seasons.find((s) => s.number === seasonNumber)
    const episode = season?.episodes.find((e) => e.number === episodeNumber)
    return of(episode)
  }
}
