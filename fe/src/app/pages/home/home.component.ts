import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterLink } from "@angular/router"
import { MovieService } from "../../core/services/movie.service"
import type {Movie, MovieItem} from "../../models/movie.model"
import {MovieItems} from '../../components/movie-items/movie-items.component';

@Component({
  selector: "app-home",
  standalone: true,
  imports: [CommonModule, RouterLink, MovieItems],
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.scss"],
})
export class HomeComponent implements OnInit {
  heroMovie: Movie | null = null
  trendingMovies: Movie[] = []
  popularMovies: MovieItem[] = []
  topRatedMovies: MovieItem[] = []
  newReleases: MovieItem[] = []

  constructor(private movieService: MovieService) {}

  ngOnInit(): void {
    this.loadMovies()
  }

  loadMovies(): void {
    this.movieService.getTrendingMovies().subscribe((movies) => {
      this.trendingMovies = movies
      this.heroMovie = movies[1]
      console.log(this.heroMovie)
    })

    this.movieService.getPopularMovies().subscribe((movies) => {
      this.popularMovies = movies
    })

    this.movieService.getTopRatedMovies().subscribe((movies) => {
      this.topRatedMovies = movies
    })

    this.movieService.getNewReleases().subscribe((movies) => {
      this.newReleases = movies
      console.log(movies)
    })
  }

  getGenreNames(movie: Movie): string {
    return movie.genres.map((g) => g.name).join(", ")
  }

  getReleaseYear(date: string): number {
    return new Date(date).getFullYear()
  }

  formatRuntime(minutes: number): string {
    const hours = Math.floor(minutes / 60)
    const mins = minutes % 60
    return `${hours}h ${mins}m`
  }
}
