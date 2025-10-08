import {Component, inject} from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterLink, RouterLinkActive } from "@angular/router"
import {FormsModule} from '@angular/forms';
import {AuthService} from '../../core/services/auth.service';

@Component({
  selector: "app-header",
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.scss"],
})
export class HeaderComponent {
  protected authService = inject(AuthService);

  isMenuOpen = false
  isSearchOpen = false
  searchQuery = ""
  isUserMenuOpen = false

  genres = [
    { id: 1, name: "Khoa Học Viễn Tưởng" },
    { id: 2, name: "Phiêu Lưu" },
    { id: 4, name: "Hình Sự" },
    { id: 6, name: "Kinh Dị" },
    { id: 7, name: "Tình Cảm" },
    { id: 9, name: "Hành Động" },
    { id: 12, name: "Hài Hước" },
  ]

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
  }

  toggleSearch(): void {
    this.isSearchOpen = !this.isSearchOpen
    if (this.isSearchOpen) {
      setTimeout(() => {
        document.getElementById("search-input")?.focus()
      }, 100)
    }
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      window.location.href = `/search?q=${encodeURIComponent(this.searchQuery)}`
    }
  }

  onLogout(): void {
    this.authService.logout()
    this.isUserMenuOpen = false
  }
}
