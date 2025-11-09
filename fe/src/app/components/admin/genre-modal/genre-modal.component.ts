import {Component, inject, OnInit} from "@angular/core"
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms"
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog"
import {Genre} from '../../../models/movie.model';
import {firstValueFrom} from 'rxjs';
import {TaxonomyService} from '../../../core/services/taxonomy.service';
import {GenreInfoComponent} from './info/genre-info.component';

@Component({
  selector: "app-genre-modal",
  templateUrl: "./genre-modal.component.html",
  styleUrls: ["./genre-modal.component.scss"],
  imports: [
    ReactiveFormsModule,
    GenreInfoComponent,
  ]
})
export class GenreModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  public dialogRef = inject(MatDialogRef<GenreModalComponent>);
  private data = inject(MAT_DIALOG_DATA) as { genreId?: string };
  private taxo = inject(TaxonomyService)

  activeTab: "info" = "info"
  isEditMode = false
  busyText = '';
  error: Error | undefined = undefined;

  infoForm!: FormGroup
  genre: Genre | null = null

  ngOnInit(): void {
    this.isEditMode = !!this.data?.genreId;

    this.initForms()
    if (this.isEditMode && this.data?.genreId) {
      this.loadGenreData()
    }
  }

  initForms(): void {
    this.infoForm = this.fb.group({
      title: ["", [Validators.required, Validators.minLength(3)]],
      icon: ["", [Validators.required]],
      description: ["", Validators.required],
    })
  }

  async loadGenreData(): Promise<void> {
    const idRaw = this.data?.genreId;
    const id = Number(idRaw);

    // đảm bảo id hợp lệ
    if (!idRaw || isNaN(id) || id <= 0) {
      console.warn('No valid movieId provided → skip load', idRaw);
      return;
    }

    try {
      const genre = await firstValueFrom(this.taxo.getGenreById(id));
      if (!genre) {
        console.warn(`Movie with id=${id} not found`);
        return;
      }

      this.genre = genre;

      this.infoForm.patchValue({
        ...genre
      });
    } catch (err) {
      console.error('Load genre failed', err);
    }
  }


  switchTab(tab: "info"): void {
    this.activeTab = tab
  }

  submit(): void {
    if (this.infoForm.valid) {
      const formValue = this.infoForm.value;
      const payload = {
        ...formValue,
      };
      console.log("[v0] Movie modal submitted:", payload);
      this.dialogRef.close(payload);
    }
  }

  cancel(): void {
    this.dialogRef.close()
  }
}
