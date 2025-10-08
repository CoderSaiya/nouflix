import {Component, inject, type OnInit, signal} from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { Router } from "@angular/router"
import type { User, UpdateProfileRequest } from "../../models/user.model"
import {AuthService} from '../../core/services/auth.service';
import {ChangePasswordModalComponent} from '../../components/change-password-modal/change-password-modal.component';
import {Title} from '@angular/platform-browser';

@Component({
  selector: "app-profile",
  standalone: true,
  imports: [CommonModule, FormsModule, ChangePasswordModalComponent],
  templateUrl: "./profile.component.html",
  styleUrls: ["./profile.component.scss"],
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService)
  private router = inject(Router)
  private titleSvc = inject(Title)

  user = signal<User | null>(null)

  // Form fields
  firstName = ""
  lastName = ""
  dateOfBirth = ""
  avatarFile: File | null = null
  avatarPreview = ""

  // Original values to track changes
  originalFirstName = ""
  originalLastName = ""
  originalDateOfBirth = ""
  originalAvatarUrl = ""

  isLoading = signal(false)
  success = signal<string | null>(null)
  error = signal<string | null>(null)

  showPasswordModal = signal(false)

  ngOnInit(): void {
    const currentUser = this.authService.currentUser()

    if (!currentUser) {
      this.router.navigate(["/login"])
      return
    }

    this.user.set(currentUser)
    this.loadUserData(currentUser)

    this.titleSvc.setTitle(`Thông tin cá nhân - ${this.user()?.firstName ?? "" + this.user()?.lastName ?? ""}`)
  }

  private loadUserData(user: User): void {
    this.firstName = user.firstName || ""
    this.lastName = user.lastName || ""
    this.dateOfBirth = user.dateOfBirth || ""
    this.avatarPreview = user.avatar || ""

    // Store original values
    this.originalFirstName = this.firstName
    this.originalLastName = this.lastName
    this.originalDateOfBirth = this.dateOfBirth
    this.originalAvatarUrl = this.avatarPreview
  }

  onAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0]

    if (file) {
      // Validate file type
      if (!file.type.startsWith("image/")) {
        this.error.set("Vui lòng chọn file ảnh")
        return
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.error.set("Kích thước ảnh không được vượt quá 5MB")
        return
      }

      this.avatarFile = file

      // Create preview
      const reader = new FileReader()
      reader.onload = (e) => {
        this.avatarPreview = e.target?.result as string
      }
      reader.readAsDataURL(file)
    }
  }

  onSubmit(): void {
    this.isLoading.set(true)
    this.error.set(null)
    this.success.set(null)

    const request: UpdateProfileRequest = {
      firstName: this.firstName !== this.originalFirstName ? this.firstName || null : null,
      lastName: this.lastName !== this.originalLastName ? this.lastName || null : null,
      dateOfBirth: this.dateOfBirth !== this.originalDateOfBirth ? this.dateOfBirth || null : null,
      avatar: this.avatarFile,
    }

    this.authService.updateProfile(request).subscribe({
      next: (updatedUser) => {
        this.authService.updateUserData(updatedUser)
        this.user.set(updatedUser)
        this.loadUserData(updatedUser)
        this.avatarFile = null
        this.success.set("Cập nhật thông tin thành công!")
        this.isLoading.set(false)

        // Clear success message after 3 seconds
        setTimeout(() => this.success.set(null), 3000)
      },
      error: (err) => {
        this.error.set(err.message || "Cập nhật thất bại")
        this.isLoading.set(false)
      },
    })
  }

  openPasswordModal(): void {
    this.showPasswordModal.set(true)
  }

  closePasswordModal(): void {
    this.showPasswordModal.set(false)
  }

  onLogout(): void {
    this.authService.logout()
  }
}
