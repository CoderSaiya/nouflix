import {Component, inject, OnInit} from "@angular/core"
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms"
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog"
import {Studio} from '../../../models/movie.model';
import {firstValueFrom} from 'rxjs';
import {TaxonomyService} from '../../../core/services/taxonomy.service';
import {StudioInfoComponent} from './info/studio-info.component';

@Component({
  selector: "app-studio-modal",
  templateUrl: "./studio-modal.component.html",
  styleUrls: ["./studio-modal.component.scss"],
  imports: [
    ReactiveFormsModule,
    StudioInfoComponent,
  ]
})
export class StudioModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  public dialogRef = inject(MatDialogRef<StudioModalComponent>);
  private data = inject(MAT_DIALOG_DATA) as { studioId?: string };
  private taxo = inject(TaxonomyService)

  activeTab: "info" = "info"
  isEditMode = false
  busyText = '';
  error: Error | undefined = undefined;

  infoForm!: FormGroup
  studio: Studio | null = null

  ngOnInit(): void {
    this.isEditMode = !!this.data?.studioId;

    this.initForms()
    if (this.isEditMode && this.data?.studioId) {
      this.loadStudioData()
    }
  }

  initForms(): void {
    this.infoForm = this.fb.group({
      title: ["", [Validators.required, Validators.minLength(3)]],
      icon: ["", [Validators.required]],
      description: ["", Validators.required],
    })
  }

  async loadStudioData(): Promise<void> {
    const idRaw = this.data?.studioId;
    const id = Number(idRaw);

    // đảm bảo id hợp lệ
    if (!idRaw || isNaN(id) || id <= 0) {
      console.warn('No valid studioId provided → skip load', idRaw);
      return;
    }

    try {
      const studio = await firstValueFrom(this.taxo.getStudioById(id));
      if (!studio) {
        console.warn(`Studio with id=${id} not found`);
        return;
      }

      this.studio = studio;

      this.infoForm.patchValue({
        ...studio
      });
    } catch (err) {
      console.error('Load studio failed', err);
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
      console.log("[v0] Studio modal submitted:", payload);
      this.dialogRef.close(payload);
    }
  }

  cancel(): void {
    this.dialogRef.close()
  }
}
