import { Component, EventEmitter, Input, Output } from "@angular/core"
import {Movie, MovieItem} from '../../../models/movie.model';
import {toYearFormat} from '../../../lib/utils/date.util';

@Component({
  selector: "app-movie-table",
  templateUrl: "./movie-table.component.html",
  styleUrls: ["./movie-table.component.scss"],
})
export class MovieTableComponent {
  @Input() movies: MovieItem[] = []
  @Input() selectedMovies: Set<number> = new Set()
  @Input() isLoading = false
  @Output() toggleSelect = new EventEmitter<string>()
  @Output() toggleSelectAll = new EventEmitter<void>()
  @Output() edit = new EventEmitter<string>()
  @Output() delete = new EventEmitter<string>()

  isAllSelectedValue(): boolean {
    return this.selectedMovies.size === this.movies.length && this.movies.length > 0
  }

  onToggleSelectAll(): void {
    this.toggleSelectAll.emit()
  }

  onToggleSelect(movieId: number): void {
    this.toggleSelect.emit(movieId.toString())
  }

  onEdit(id: number): void {
    this.edit.emit(id.toString())
  }

  onDelete(id: number): void {
    this.delete.emit(id.toString())
  }

  getStatusClass(status: string): string {
    return `status-${status}`
  }

  protected readonly toYearFormat = toYearFormat;
}
