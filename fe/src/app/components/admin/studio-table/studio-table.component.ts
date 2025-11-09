import { Component, EventEmitter, Input, Output } from "@angular/core"
import {Genre, Studio} from '../../../models/movie.model';

@Component({
  selector: "app-studio-table",
  templateUrl: "./studio-table.component.html",
  styleUrls: ["./studio-table.component.scss"],
})
export class StudioTableComponent {
  @Input() studios: Studio[] = []
  @Input() selectedStudios: Set<number> = new Set()
  @Input() isLoading = false
  @Output() toggleSelect = new EventEmitter<number>()
  @Output() toggleSelectAll = new EventEmitter<void>()
  @Output() edit = new EventEmitter<number>()
  @Output() delete = new EventEmitter<number>()

  isAllSelectedValue(): boolean {
    return this.selectedStudios.size === this.studios.length && this.studios.length > 0
  }

  onToggleSelectAll(): void {
    this.toggleSelectAll.emit()
  }

  onToggleSelect(studioId: number): void {
    this.toggleSelect.emit(studioId)
  }

  onEdit(id: number): void {
    this.edit.emit(id)
  }

  onDelete(id: number): void {
    this.delete.emit(id)
  }
}
