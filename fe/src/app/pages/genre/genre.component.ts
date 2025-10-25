import {Component, inject, type OnInit} from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, ActivatedRoute } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { Title, Meta } from "@angular/platform-browser"
import { MovieService } from "../../core/services/movie.service"
import type {Genre, Movie, MovieItem} from "../../models/movie.model"
import {TaxonomyService} from '../../core/services/taxonomy.service';
import {MovieItems} from '../../components/movie-items/movie-items.component';

@Component({
  selector: "app-genre",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, MovieItems],
  templateUrl: "./genre.component.html",
  styleUrl: "./genre.component.scss",
})
export class GenreComponent implements OnInit {
  private taxSvc = inject(TaxonomyService)
  private movSvc = inject(MovieService)
  private titSvc = inject(Title)
  private metaSvc = inject(Meta)
  private route = inject(ActivatedRoute)

  movies: MovieItem[] = []
  filteredMovies: MovieItem[] = []
  allGenres: Genre[] = []
  currentGenre: { id: number; name: string } | null = null
  isLoading = true

  sortBy = "release"
  sortOptions = [
    { value: "release", label: "Mới Nhất" },
    { value: "rating", label: "Đánh Giá Cao" },
    { value: "title", label: "Tên A-Z" },
  ]

  // allGenres = [
  //   { id: 1, name: "Khoa Học Viễn Tưởng", icon: "🚀" },
  //   { id: 2, name: "Phiêu Lưu", icon: "🗺️" },
  //   { id: 3, name: "Kịch Tính", icon: "🎭" },
  //   { id: 4, name: "Hình Sự", icon: "🔫" },
  //   { id: 5, name: "Bí Ẩn", icon: "🔍" },
  //   { id: 6, name: "Kinh Dị", icon: "👻" },
  //   { id: 7, name: "Tình Cảm", icon: "❤️" },
  //   { id: 8, name: "Chính Kịch", icon: "🎬" },
  //   { id: 9, name: "Hành Động", icon: "💥" },
  //   { id: 10, name: "Giật Gân", icon: "😱" },
  //   { id: 11, name: "Nhạc Kịch", icon: "🎵" },
  //   { id: 12, name: "Hài Hước", icon: "😂" },
  //   { id: 13, name: "Gia Đình", icon: "👨‍👩‍👧‍👦" },
  //   { id: 14, name: "Thể Thao", icon: "⚽" },
  // ]

  ngOnInit(): void {
    this.isLoading = true;

    this.taxSvc.getGenres('').subscribe(genres => {
      this.allGenres = genres;

      this.route.params.subscribe((params) => {
        const genreParam = params["genre"]

        // Check if it's a genre ID or name
        const genreId = Number.parseInt(genreParam)
        console.log(genreId)
        if (!isNaN(genreId)) {
          this.currentGenre = this.allGenres.find((g) => g.id === genreId) || null
          console.log(this.currentGenre)
        } else {
          this.currentGenre = this.allGenres.find((g) => g.name.toLowerCase() === genreParam.toLowerCase()) || null
        }

        if (this.currentGenre) {
          this.loadMoviesByGenre(this.currentGenre.id)
          this.updateMetaTags()
        }
      })
    });
  }

  updateMetaTags(): void {
    if (!this.currentGenre) return

    const title = `Phim ${this.currentGenre.name} - NouFlix`
    const description = `Khám phá bộ sưu tập phim ${this.currentGenre.name} chất lượng cao với đầy đủ thông tin và đánh giá chi tiết`

    this.titSvc.setTitle(title)
    this.metaSvc.updateTag({ name: "description", content: description })
    this.metaSvc.updateTag({ property: "og:title", content: title })
    this.metaSvc.updateTag({ property: "og:description", content: description })
  }

  loadMoviesByGenre(genreId: number): void {
    this.isLoading = true
    this.movSvc.getMoviesByGenre(genreId).subscribe((movies) => {
      this.movies = movies
      console.log(movies)
      this.filteredMovies = this.sortMovies([...movies])
      this.isLoading = false
    })
  }

  onSortChange(): void {
    this.filteredMovies = this.sortMovies([...this.movies])
  }

  sortMovies(movies: MovieItem[]): MovieItem[] {
    switch (this.sortBy) {
      case "rating":
        return movies.sort((a, b) => b.avgRating - a.avgRating)
      case "release":
        return movies.sort((a, b) => new Date(b.releaseDate).getTime() - new Date(a.releaseDate).getTime())
      case "title":
        return movies.sort((a, b) => a.title.localeCompare(b.title))
      default:
        return movies
    }
  }

  getReleaseYear(date: string): string {
    return new Date(date).getFullYear().toString()
  }

  getGenreIcon(genreId: number): string {
    return this.allGenres.find((g) => g.id === genreId)?.icon || "🎬"
  }

  getAverageRating(): string {
    if (this.filteredMovies.length === 0) return "0.0"
    const sum = this.filteredMovies.reduce((acc, movie) => acc + movie.avgRating, 0)
    return (sum / this.filteredMovies.length).toFixed(1)
  }
}
