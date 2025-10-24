import {Component, inject, type OnInit} from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, ActivatedRoute, Router } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { Title, Meta } from "@angular/platform-browser"
import { MovieService } from "../../core/services/movie.service"
import type {Genre, Movie} from "../../models/movie.model"
import {MovieItems} from '../../components/movie-items/movie-items.component';
import {TaxonomyService} from '../../core/services/taxonomy.service';

@Component({
  selector: "app-search",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, MovieItems],
  templateUrl: "./search.component.html",
  styleUrl: "./search.component.scss",
})
export class SearchComponent implements OnInit {
  private movSvc = inject(MovieService)
  private router = inject(Router)
  private route = inject(ActivatedRoute)
  private titSvc = inject(Title)
  private metaSvc = inject(Meta)
  private taxSvc = inject(TaxonomyService)

  searchQuery = ""
  movies: Movie[] = []
  filteredMovies: Movie[] = []
  searchResults: Movie[] = []
  genres: Genre[] = []
  isLoading = false
  hasSearched = false

  // Filter options
  selectedGenre : number | null = null
  selectedYear = ""
  selectedRating = ""
  sortBy = "popularity"

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

  constructor() {
    let date = new Date();
    const currentYear = date.getFullYear()
    for (let year = currentYear; year >= 2000; year--) {
      this.years.push(year.toString())
    }
  }

  ngOnInit(): void {
    this.updateMetaTags()

    // Get query from URL params
    this.route.queryParams.subscribe((params) => {
      const q = params['q'] ?? '';
      if (q !== this.searchQuery) {
        this.searchQuery = q;
        if (this.searchQuery.trim()) {
          this.performSearch();
        } else {
          this.searchResults = [];
          this.filteredMovies = [];
          this.hasSearched = false;
        }
      }
    })

    this.loadAllMovies()
  }

  updateMetaTags(): void {
    const title = this.searchQuery ? `Tìm Kiếm: ${this.searchQuery} - NouFlix` : "Tìm Kiếm Phim - NouFlix"
    const description = "Tìm kiếm phim yêu thích của bạn với bộ lọc thông minh theo thể loại, năm phát hành và đánh giá"

    this.titSvc.setTitle(title)
    this.metaSvc.updateTag({ name: "description", content: description })
    this.metaSvc.updateTag({ property: "og:title", content: title })
    this.metaSvc.updateTag({ property: "og:description", content: description })
  }

  loadAllMovies(): void {
    this.movSvc.getTrendingMovies().subscribe((movies) => {
      this.movies = movies ?? [];
    })

    this.taxSvc.getGenres().subscribe((genres) => {
      this.genres = genres
    })
  }

  performSearch(): void {
    if (!this.searchQuery.trim()) {
      this.searchResults = [];
      this.filteredMovies = []
      this.hasSearched = false
      return
    }

    this.isLoading = true
    this.hasSearched = true

    this.movSvc.searchMovies(this.searchQuery).subscribe((movies) => {
      this.searchResults = movies ?? []
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
    let results = [...this.searchResults]

    // Filter by genre
    if (this.selectedGenre !== null) {
      results = results.filter(m =>
        m.genres?.some(g => g.id === this.selectedGenre)
      );
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
        return movies.sort((a, b) =>
          (a.title ?? "").localeCompare(b.title ?? "", "vi", { sensitivity: "accent" })
        );
      default:
        return movies
    }
  }

  onFilterChange(): void {
    this.applyFilters()
  }

  clearFilters(): void {
    this.selectedGenre = null
    this.selectedYear = ""
    this.selectedRating = ""
    this.sortBy = "popularity"
    this.applyFilters()
  }

  getReleaseYear(date: string): string {
    return new Date(date).getFullYear().toString()
  }
}
