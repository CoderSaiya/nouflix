import {Component, inject, type OnInit} from "@angular/core"
import { FormBuilder, type FormGroup, ReactiveFormsModule, Validators} from "@angular/forms"

@Component({
  selector: "app-settings",
  templateUrl: "./settings.component.html",
  styleUrls: ["./settings.component.scss"],
  imports: [
    ReactiveFormsModule
  ]
})
export class SettingsComponent implements OnInit {
  private fb = inject(FormBuilder)

  settingsForm!: FormGroup
  isSaving = false
  saveSuccess = false

  languageOptions = [
    { value: "vi", label: "Tiếng Việt" },
    { value: "en", label: "English" },
    { value: "ja", label: "日本語" },
  ]

  themeOptions = [
    { value: "dark", label: "Tối" },
    { value: "light", label: "Sáng" },
    { value: "auto", label: "Tự Động" },
  ]

  ngOnInit(): void {
    this.initForm()
  }

  initForm(): void {
    this.settingsForm = this.fb.group({
      siteName: ["NouFlix Admin", [Validators.required, Validators.minLength(3)]],
      siteDescription: ["Quản lý nội dung phim trực tuyến", Validators.required],
      logo: [""],
      favicon: [""],
      defaultLanguage: ["vi", Validators.required],
      theme: ["dark", Validators.required],
      itemsPerPage: [10, [Validators.required, Validators.min(5), Validators.max(100)]],
      enableNotifications: [true],
      enableAnalytics: [true],
      maintenanceMode: [false],
      supportEmail: ["support@nouflix.com", [Validators.required, Validators.email]],
      contactEmail: ["contact@nouflix.com", [Validators.required, Validators.email]],
      maxUploadSize: [500, [Validators.required, Validators.min(1)]],
      allowedVideoFormats: ["mp4,mkv,avi"],
      allowedImageFormats: ["jpg,png,webp"],
    })
  }

  onFileSelected(event: Event, field: string): void {
    const input = event.target as HTMLInputElement
    if (input.files?.[0]) {
      console.log("[v0] File selected for", field, ":", input.files[0].name)
      this.settingsForm.patchValue({ [field]: input.files[0].name })
    }
  }

  saveSettings(): void {
    if (this.settingsForm.valid) {
      this.isSaving = true
      console.log("[v0] Saving settings:", this.settingsForm.value)

      setTimeout(() => {
        this.isSaving = false
        this.saveSuccess = true
        setTimeout(() => {
          this.saveSuccess = false
        }, 3000)
      }, 1500)
    }
  }

  resetSettings(): void {
    this.settingsForm.reset({
      siteName: "NouFlix Admin",
      siteDescription: "Quản lý nội dung phim trực tuyến",
      defaultLanguage: "vi",
      theme: "dark",
      itemsPerPage: 10,
      enableNotifications: true,
      enableAnalytics: true,
      maintenanceMode: false,
      supportEmail: "support@nouflix.com",
      contactEmail: "contact@nouflix.com",
      maxUploadSize: 500,
      allowedVideoFormats: "mp4,mkv,avi",
      allowedImageFormats: "jpg,png,webp",
    })
  }
}
