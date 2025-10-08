import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, ActivatedRoute, Router } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { Title, Meta } from "@angular/platform-browser"
import { MovieService } from "../../core/services/movie.service"
import type { Movie } from "../../models/movie.model"

@Component({
  selector: "app-search",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: "./search.component.html",
  styleUrl: "./search.component.scss",
})
export class SearchComponent implements OnInit {
  searchQuery = ""
  movies: Movie[] = []
  filteredMovies: Movie[] = []
  isLoading = false
  hasSearched = false

  // Filter options
  selectedGenre = ""
  selectedYear = ""
  selectedRating = ""
  sortBy = "popularity"

  genres: { id: number; name: string }[] = [
    { id: 1, name: "Khoa Học Viễn Tưởng" },
    { id: 2, name: "Phiêu Lưu" },
    { id: 3, name: "Kịch Tính" },
    { id: 4, name: "Hình Sự" },
    { id: 5, name: "Bí Ẩn" },
    { id: 6, name: "Kinh Dị" },
    { id: 7, name: "Tình Cảm" },
    { id: 8, name: "Chính Kịch" },
    { id: 9, name: "Hành Động" },
    { id: 10, name: "Giật Gân" },
    { id: 11, name: "Nhạc Kịch" },
    { id: 12, name: "Hài Hước" },
    { id: 13, name: "Gia Đình" },
    { id: 14, name: "Thể Thao" },
  ]

  years: string[] = []
  ratings = [
    { value: "", label: "Tất Cả" },
    { value: "9", label: "9+ Xuất Sắc" },
    { value: "8", label: "8+ Rất Tốt" },
    { value: "7", label: "7+ Tốt" },
    { value: "6", label: "6+ Khá" },
  ]

  sortOptions = [
    { value: "popularity", label: "Phổ Biến Nhất" },
    { value: "rating", label: "Đánh Giá Cao" },
    { value: "release", label: "Mới Nhất" },
    { value: "title", label: "Tên A-Z" },
  ]

  constructor(
    private movieService: MovieService,
    private route: ActivatedRoute,
    private router: Router,
    private titleService: Title,
    private metaService: Meta,
  ) {
    // Generate years from 2024 to 2000
    const currentYear = 2024
    for (let year = currentYear; year >= 2000; year--) {
      this.years.push(year.toString())
    }
  }

  ngOnInit(): void {
    this.updateMetaTags()

    // Get query from URL params
    this.route.queryParams.subscribe((params) => {
      if (params["q"]) {
        this.searchQuery = params["q"]
        this.performSearch()
      }
    })

    // Load all movies for filtering
    this.loadAllMovies()
  }

  updateMetaTags(): void {
    const title = this.searchQuery ? `Tìm Kiếm: ${this.searchQuery} - NouFlix` : "Tìm Kiếm Phim - NouFlix"
    const description = "Tìm kiếm phim yêu thích của bạn với bộ lọc thông minh theo thể loại, năm phát hành và đánh giá"

    this.titleService.setTitle(title)
    this.metaService.updateTag({ name: "description", content: description })
    this.metaService.updateTag({ property: "og:title", content: title })
    this.metaService.updateTag({ property: "og:description", content: description })
  }

  loadAllMovies(): void {
    this.movieService.getAllMovies().subscribe((movies) => {
      this.movies = movies
    })
  }

  performSearch(): void {
    if (!this.searchQuery.trim()) {
      this.filteredMovies = []
      this.hasSearched = false
      return
    }

    this.isLoading = true
    this.hasSearched = true

    this.movieService.searchMovies(this.searchQuery).subscribe((movies) => {
      this.filteredMovies = movies
      this.applyFilters()
      this.isLoading = false
      this.updateMetaTags()
    })
  }

  onSearch(): void {
    this.router.navigate(["/search"], {
      queryParams: { q: this.searchQuery },
    })
    this.performSearch()
  }

  applyFilters(): void {
    let results = [...this.filteredMovies]

    // Filter by genre
    if (this.selectedGenre) {
      const genreId = Number.parseInt(this.selectedGenre)
      results = results.filter((movie) => movie.genres.some((g) => g.id === genreId))
    }

    // Filter by year
    if (this.selectedYear) {
      results = results.filter((movie) => movie.releaseDate.startsWith(this.selectedYear))
    }

    // Filter by rating
    if (this.selectedRating) {
      const minRating = Number.parseFloat(this.selectedRating)
      results = results.filter((movie) => movie.avgRating >= minRating)
    }

    // Sort results
    results = this.sortMovies(results)

    this.filteredMovies = results
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

  onFilterChange(): void {
    this.applyFilters()
  }

  clearFilters(): void {
    this.selectedGenre = ""
    this.selectedYear = ""
    this.selectedRating = ""
    this.sortBy = "popularity"
    this.applyFilters()
  }

  getReleaseYear(date: string): string {
    return new Date(date).getFullYear().toString()
  }
}
