import {Component, inject, type OnInit} from "@angular/core"
import { Router } from "@angular/router"
import {MatDialog} from '@angular/material/dialog';
import {Genre} from '../../../models/movie.model';
import {TaxonomyService} from '../../../core/services/taxonomy.service';
import {GenreTableComponent} from '../../../components/admin/genre-table/genre-table.component';
import {GenreModalComponent} from '../../../components/admin/genre-modal/genre-modal.component';

@Component({
  selector: "app-genres-list",
  templateUrl: "./genres.component.html",
  styleUrls: ["./genres.component.scss"],
  imports: [
    GenreTableComponent
  ]
})
export class GenresListComponent implements OnInit {
  genres: Genre[] = []
  isLoading = true
  selectedGenre: Set<number> = new Set()
  isDeleteConfirmOpen = false
  genreToDelete: number | null = null

  private tax = inject(TaxonomyService)
  private router = inject(Router)
  private dialog = inject(MatDialog)

  ngOnInit(): void {
    this.loadGenres()
  }

  loadGenres(): void {
    this.isLoading = true
    this.tax.getGenres().subscribe({
      next: (data) => {
        this.genres = data
        this.isLoading = false
      },
      error: () => {
        this.isLoading = false
      },
    })
  }

  toggleSelectGenre(genreId: number): void {
    if (this.selectedGenre.has(genreId)) {
      this.selectedGenre.delete(genreId)
    } else {
      this.selectedGenre.add(genreId)
    }
  }

  toggleSelectAll(): void {
    if (this.selectedGenre.size === this.genres.length) {
      this.selectedGenre.clear()
    } else {
      this.genres.forEach((m) => this.selectedGenre.add(m.id))
    }
  }

  edit(id: number): void {
    const dialogRef = this.dialog.open(GenreModalComponent, {
      width: "90vw",
      maxWidth: "1000px",
      height: "90vh",
      maxHeight: "800px",
      data: { genreId: id },
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadGenres()
      }
    })
  }

  openDeleteConfirm(id: number): void {
    this.genreToDelete = id
    this.isDeleteConfirmOpen = true
  }

  confirmDelete(): void {
    if (this.genreToDelete) {
      this.genres = this.genres.filter((m) => m.id !== this.genreToDelete)
      this.selectedGenre.delete(this.genreToDelete)
      this.isDeleteConfirmOpen = false
      this.genreToDelete = null
    }
  }

  cancelDelete(): void {
    this.isDeleteConfirmOpen = false
    this.genreToDelete = null
  }

  deleteSelected(): void {
    this.genres = this.genres.filter((m) => !this.selectedGenre.has(m.id))
    this.selectedGenre.clear()
  }

  create(): void {
    const dialogRef = this.dialog.open(GenreModalComponent, {
      width: "90vw",
      maxWidth: "1000px",
      height: "90vh",
      maxHeight: "800px",
      data: { genreId: null },
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadGenres()
      }
    })
  }

  protected readonly Number = Number;
}
