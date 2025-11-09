import {Component, EventEmitter, inject, Input, Output} from "@angular/core"
import {AuthService} from '../../../core/services/auth.service';

@Component({
  selector: "app-header",
  standalone: true,
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.scss"],
})
export class HeaderComponent {
  @Input() isSidebarCollapsed = false
  @Output() toggleSidebar = new EventEmitter<void>()

  private authService = inject(AuthService)

  isUserMenuOpen = false

  onToggleSidebar() {
    this.toggleSidebar.emit()
  }

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen
  }

  onLogout(): void {
    this.authService.logout()
    this.isUserMenuOpen = false
  }
}
