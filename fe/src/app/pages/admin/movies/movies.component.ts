import {Component, inject, type OnInit} from "@angular/core"
import { Router } from "@angular/router"
import {AdminService} from '../../../core/services/admin.service';
import {MovieTableComponent} from '../../../components/admin/movie-table/movie-table.component';
import {MatDialog} from '@angular/material/dialog';
import {MovieModalComponent} from '../../../components/admin/movie-modal/movie-modal.component';
import {MovieService} from '../../../core/services/movie.service';
import {Movie, MovieItem} from '../../../models/movie.model';

@Component({
  selector: "app-movies-list",
  templateUrl: "./movies.component.html",
  styleUrls: ["./movies.component.scss"],
  imports: [
    MovieTableComponent
  ]
})
export class MoviesListComponent implements OnInit {
  movies: MovieItem[] = []
  isLoading = true
  selectedMovies: Set<number> = new Set()
  isDeleteConfirmOpen = false
  movieToDelete: number | null = null

  private movieService = inject(MovieService)
  private router = inject(Router)
  private dialog = inject(MatDialog)

  ngOnInit(): void {
    this.loadMovies()
  }

  loadMovies(): void {
    this.isLoading = true
    this.movieService.searchMovies().subscribe({
      next: (data) => {
        this.movies = data
        this.isLoading = false
      },
      error: () => {
        this.isLoading = false
      },
    })
  }

  toggleSelectMovie(movieId: number): void {
    if (this.selectedMovies.has(movieId)) {
      this.selectedMovies.delete(movieId)
    } else {
      this.selectedMovies.add(movieId)
    }
  }

  toggleSelectAll(): void {
    if (this.selectedMovies.size === this.movies.length) {
      this.selectedMovies.clear()
    } else {
      this.movies.forEach((m) => this.selectedMovies.add(m.id))
    }
  }

  editMovie(id: string): void {
    const dialogRef = this.dialog.open(MovieModalComponent, {
      width: "90vw",
      maxWidth: "1000px",
      height: "90vh",
      maxHeight: "800px",
      data: { movieId: id },
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadMovies()
      }
    })
  }

  openDeleteConfirm(id: number): void {
    this.movieToDelete = id
    this.isDeleteConfirmOpen = true
  }

  confirmDelete(): void {
    if (this.movieToDelete) {
      this.movies = this.movies.filter((m) => m.id !== this.movieToDelete)
      this.selectedMovies.delete(this.movieToDelete)
      this.isDeleteConfirmOpen = false
      this.movieToDelete = null
    }
  }

  cancelDelete(): void {
    this.isDeleteConfirmOpen = false
    this.movieToDelete = null
  }

  deleteSelected(): void {
    this.movies = this.movies.filter((m) => !this.selectedMovies.has(m.id))
    this.selectedMovies.clear()
  }

  createMovie(): void {
    this.router.navigate(["/admin/movies/create"])
  }

  protected readonly Number = Number;
}
