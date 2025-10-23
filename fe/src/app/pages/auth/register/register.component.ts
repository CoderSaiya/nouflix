import {Component, inject, signal} from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { Router, RouterModule } from "@angular/router"
import type { RegisterRequest } from "../../../models/user.model"
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: "app-register",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.scss"],
})
export class RegisterComponent {
  private authSvc = inject(AuthService)
  private router = inject(Router)

  email = ""
  password = ""
  confirmPassword = ""
  isLoading = signal(false)
  error = signal<string | null>(null)
  showPassword = signal(false)
  showConfirmPassword = signal(false)

  onSubmit(): void {
    if (!this.email || !this.password || !this.confirmPassword) {
      this.error.set("Vui lòng điền đầy đủ thông tin")
      return
    }

    if (this.password !== this.confirmPassword) {
      this.error.set("Mật khẩu xác nhận không khớp")
      return
    }

    if (this.password.length < 8) {
      this.error.set("Mật khẩu phải có ít nhất 8 ký tự")
      return
    }

    this.isLoading.set(true)
    this.error.set(null)

    const request: RegisterRequest = {
      email: this.email,
      password: this.password
    }

    this.authSvc.register(request).subscribe({
      next: (response) => {
        // this.authSvc.setAuthData(response)
        this.router.navigate(["/profile"])
      },
      error: (err) => {
        this.error.set(err.message || "Đăng ký thất bại")
        this.isLoading.set(false)
      },
      complete: () => {
        this.isLoading.set(false)
      },
    })
  }

  onSocialLogin(provider: "google" | "facebook"): void {
    this.isLoading.set(true)
    this.error.set(null)

    // this.authSvc.socialLogin({ provider, token: "mock-token" }).subscribe({
    //   next: (response) => {
    //     this.authSvc.setAuthData(response)
    //     this.router.navigate(["/"])
    //   },
    //   error: (err) => {
    //     this.error.set(err.message || "Đăng ký thất bại")
    //     this.isLoading.set(false)
    //   },
    //   complete: () => {
    //     this.isLoading.set(false)
    //   },
    // })
  }

  togglePasswordVisibility(field: "password" | "confirm"): void {
    if (field === "password") {
      this.showPassword.set(!this.showPassword())
    } else {
      this.showConfirmPassword.set(!this.showConfirmPassword())
    }
  }
}
