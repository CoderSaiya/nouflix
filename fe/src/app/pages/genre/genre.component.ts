import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, ActivatedRoute } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { Title, Meta } from "@angular/platform-browser"
import { MovieService } from "../../core/services/movie.service"
import type { Movie } from "../../models/movie.model"

@Component({
  selector: "app-genre",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: "./genre.component.html",
  styleUrl: "./genre.component.scss",
})
export class GenreComponent implements OnInit {
  movies: Movie[] = []
  filteredMovies: Movie[] = []
  currentGenre: { id: number; name: string } | null = null
  isLoading = true

  sortBy = "popularity"
  sortOptions = [
    { value: "popularity", label: "Phổ Biến Nhất" },
    { value: "rating", label: "Đánh Giá Cao" },
    { value: "release", label: "Mới Nhất" },
    { value: "title", label: "Tên A-Z" },
  ]

  allGenres = [
    { id: 1, name: "Khoa Học Viễn Tưởng", icon: "🚀" },
    { id: 2, name: "Phiêu Lưu", icon: "🗺️" },
    { id: 3, name: "Kịch Tính", icon: "🎭" },
    { id: 4, name: "Hình Sự", icon: "🔫" },
    { id: 5, name: "Bí Ẩn", icon: "🔍" },
    { id: 6, name: "Kinh Dị", icon: "👻" },
    { id: 7, name: "Tình Cảm", icon: "❤️" },
    { id: 8, name: "Chính Kịch", icon: "🎬" },
    { id: 9, name: "Hành Động", icon: "💥" },
    { id: 10, name: "Giật Gân", icon: "😱" },
    { id: 11, name: "Nhạc Kịch", icon: "🎵" },
    { id: 12, name: "Hài Hước", icon: "😂" },
    { id: 13, name: "Gia Đình", icon: "👨‍👩‍👧‍👦" },
    { id: 14, name: "Thể Thao", icon: "⚽" },
  ]

  constructor(
    private movieService: MovieService,
    private route: ActivatedRoute,
    private titleService: Title,
    private metaService: Meta,
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const genreParam = params["genre"]

      // Check if it's a genre ID or name
      const genreId = Number.parseInt(genreParam)
      if (!isNaN(genreId)) {
        this.currentGenre = this.allGenres.find((g) => g.id === genreId) || null
      } else {
        this.currentGenre = this.allGenres.find((g) => g.name.toLowerCase() === genreParam.toLowerCase()) || null
      }

      if (this.currentGenre) {
        this.loadMoviesByGenre(this.currentGenre.id)
        this.updateMetaTags()
      }
    })
  }

  updateMetaTags(): void {
    if (!this.currentGenre) return

    const title = `Phim ${this.currentGenre.name} - NouFlix`
    const description = `Khám phá bộ sưu tập phim ${this.currentGenre.name} chất lượng cao với đầy đủ thông tin và đánh giá chi tiết`

    this.titleService.setTitle(title)
    this.metaService.updateTag({ name: "description", content: description })
    this.metaService.updateTag({ property: "og:title", content: title })
    this.metaService.updateTag({ property: "og:description", content: description })
  }

  loadMoviesByGenre(genreId: number): void {
    this.isLoading = true
    this.movieService.getMoviesByGenre(genreId).subscribe((movies) => {
      this.movies = movies
      this.filteredMovies = this.sortMovies([...movies])
      this.isLoading = false
    })
  }

  onSortChange(): void {
    this.filteredMovies = this.sortMovies([...this.movies])
  }

  sortMovies(movies: Movie[]): Movie[] {
    switch (this.sortBy) {
      case "popularity":
        return movies.sort((a, b) => b.popularity - a.popularity)
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
