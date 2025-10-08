import {Component, EventEmitter, inject, Output, signal} from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import type { ChangePasswordRequest } from "../../models/user.model"
import {AuthService} from '../../core/services/auth.service';

@Component({
  selector: "app-change-password-modal",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./change-password-modal.component.html",
  styleUrls: ["./change-password-modal.component.scss"],
})
export class ChangePasswordModalComponent {
  @Output() close = new EventEmitter<void>()

  private authService = inject(AuthService)

  currentPassword = ""
  newPassword = ""
  confirmNewPassword = ""

  showCurrentPassword = signal(false)
  showNewPassword = signal(false)
  showConfirmPassword = signal(false)

  isLoading = signal(false)
  success = signal<string | null>(null)
  error = signal<string | null>(null)

  onSubmit(): void {
    if (!this.currentPassword || !this.newPassword || !this.confirmNewPassword) {
      this.error.set("Vui lòng điền đầy đủ thông tin")
      return
    }

    if (this.newPassword !== this.confirmNewPassword) {
      this.error.set("Mật khẩu mới không khớp")
      return
    }

    if (this.newPassword.length < 8) {
      this.error.set("Mật khẩu phải có ít nhất 8 ký tự")
      return
    }

    if (this.newPassword === this.currentPassword) {
      this.error.set("Mật khẩu mới phải khác mật khẩu hiện tại")
      return
    }

    this.isLoading.set(true)
    this.error.set(null)

    const request: ChangePasswordRequest = {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }

    this.authService.changePassword(request).subscribe({
      next: () => {
        this.success.set("Đổi mật khẩu thành công!")
        setTimeout(() => {
          this.close.emit()
        }, 1500)
      },
      error: (err) => {
        this.error.set(err.message || "Đổi mật khẩu thất bại")
        this.isLoading.set(false)
      },
      complete: () => {
        this.isLoading.set(false)
      },
    })
  }

  onClose(): void {
    this.close.emit()
  }

  togglePasswordVisibility(field: "current" | "new" | "confirm"): void {
    if (field === "current") {
      this.showCurrentPassword.set(!this.showCurrentPassword())
    } else if (field === "new") {
      this.showNewPassword.set(!this.showNewPassword())
    } else {
      this.showConfirmPassword.set(!this.showConfirmPassword())
    }
  }
}
