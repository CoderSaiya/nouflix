import {Component, inject, OnInit, signal} from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { Router, RouterModule } from "@angular/router"
import type { LoginRequest } from "../../../models/user.model"
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: "app-login",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit{
  private authService= inject(AuthService)
  private router= inject(Router)

  email = ""
  password = ""
  isLoading = signal(false)
  error = signal<string | null>(null)
  showPassword = signal(false)

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.error.set("Vui lòng điền đầy đủ thông tin")
      return
    }

    this.isLoading.set(true)
    this.error.set(null)

    const request: LoginRequest = {
      email: this.email,
      password: this.password,
    }

    this.authService.login(request).subscribe({
      next: (response) => {
        this.authService.setAuthData(response)
        console.log(response)
        response.user.role.toLowerCase() === 'user' ?
          this.router.navigate(["/"]) :
          this.router.navigate(["/admin"])
      },
      error: (err) => {
        this.error.set(err.detail || err.message  || "Đăng nhập thất bại")
        this.isLoading.set(false)
      },
      complete: () => {
        this.isLoading.set(false)
      },
    })
  }

  onSocialLogin(provider: "google" | "facebook"): void {
    // this.isLoading.set(true)
    // this.error.set(null)
    //
    // // In production, this would open OAuth popup/redirect
    // this.authService.socialLogin({ provider, token: "mock-token" }).subscribe({
    //   next: (response) => {
    //     this.authService.setAuthData(response)
    //     this.router.navigate(["/"])
    //   },
    //   error: (err) => {
    //     this.error.set(err.message || "Đăng nhập thất bại")
    //     this.isLoading.set(false)
    //   },
    //   complete: () => {
    //     this.isLoading.set(false)
    //   },
    // })

    this.error.set(null);
    this.isLoading.set(true);
    this.authService.loginWithProvider(provider);
  }

  togglePasswordVisibility(): void {
    this.showPassword.set(!this.showPassword())
  }

  ngOnInit(): void {
  }
}
