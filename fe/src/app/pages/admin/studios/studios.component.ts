import {Component, inject, type OnInit} from "@angular/core"
import {MatDialog} from '@angular/material/dialog';
import {Studio} from '../../../models/movie.model';
import {TaxonomyService} from '../../../core/services/taxonomy.service';
import {StudioTableComponent} from '../../../components/admin/studio-table/studio-table.component';
import {StudioModalComponent} from '../../../components/admin/studio-modal/studio-modal.component';

@Component({
  selector: "app-studios-list",
  templateUrl: "./studios.component.html",
  styleUrls: ["./studios.component.scss"],
  imports: [
    StudioTableComponent
  ]
})
export class StudiosListComponent implements OnInit {
  studios: Studio[] = []
  isLoading = true
  selectedStudio: Set<number> = new Set()
  isDeleteConfirmOpen = false
  studioToDelete: number | null = null

  private tax = inject(TaxonomyService)
  private dialog = inject(MatDialog)

  ngOnInit(): void {
    this.loadStudios()
  }

  loadStudios(): void {
    this.isLoading = true
    this.tax.getStudios().subscribe({
      next: (data) => {
        this.studios = data
        this.isLoading = false
      },
      error: () => {
        this.isLoading = false
      },
    })
  }

  toggleSelectStudio(studioId: number): void {
    if (this.selectedStudio.has(studioId)) {
      this.selectedStudio.delete(studioId)
    } else {
      this.selectedStudio.add(studioId)
    }
  }

  toggleSelectAll(): void {
    if (this.selectedStudio.size === this.studios.length) {
      this.selectedStudio.clear()
    } else {
      this.studios.forEach((m) => this.selectedStudio.add(m.id))
    }
  }

  edit(id: number): void {
    const dialogRef = this.dialog.open(StudioModalComponent, {
      width: "90vw",
      maxWidth: "1000px",
      height: "90vh",
      maxHeight: "800px",
      data: { studioId: id },
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadStudios()
      }
    })
  }

  openDeleteConfirm(id: number): void {
    this.studioToDelete = id
    this.isDeleteConfirmOpen = true
  }

  confirmDelete(): void {
    if (this.studioToDelete) {
      this.studios = this.studios.filter((m) => m.id !== this.studioToDelete)
      this.selectedStudio.delete(this.studioToDelete)
      this.isDeleteConfirmOpen = false
      this.studioToDelete = null
    }
  }

  cancelDelete(): void {
    this.isDeleteConfirmOpen = false
    this.studioToDelete = null
  }

  deleteSelected(): void {
    this.studios = this.studios.filter((m) => !this.selectedStudio.has(m.id))
    this.selectedStudio.clear()
  }

  create(): void {
    const dialogRef = this.dialog.open(StudioModalComponent, {
      width: "90vw",
      maxWidth: "1000px",
      height: "90vh",
      maxHeight: "800px",
      data: { studioId: null },
    })

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadStudios()
      }
    })
  }

  protected readonly Number = Number;
}
