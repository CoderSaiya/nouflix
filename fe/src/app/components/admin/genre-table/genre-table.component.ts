import { Component, EventEmitter, Input, Output } from "@angular/core"
import {Genre} from '../../../models/movie.model';

@Component({
  selector: "app-genre-table",
  templateUrl: "./genre-table.component.html",
  styleUrls: ["./genre-table.component.scss"],
})
export class GenreTableComponent {
  @Input() genres: Genre[] = []
  @Input() selectedGenres: Set<number> = new Set()
  @Input() isLoading = false
  @Output() toggleSelect = new EventEmitter<number>()
  @Output() toggleSelectAll = new EventEmitter<void>()
  @Output() edit = new EventEmitter<number>()
  @Output() delete = new EventEmitter<number>()

  isAllSelectedValue(): boolean {
    return this.selectedGenres.size === this.genres.length && this.genres.length > 0
  }

  onToggleSelectAll(): void {
    this.toggleSelectAll.emit()
  }

  onToggleSelect(genreId: number): void {
    this.toggleSelect.emit(genreId)
  }

  onEdit(id: number): void {
    this.edit.emit(id)
  }

  onDelete(id: number): void {
    this.delete.emit(id)
  }
}
