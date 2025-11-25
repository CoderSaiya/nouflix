import { Component, ElementRef, inject, Input, ViewChild } from "@angular/core"
import { Router, RouterLink, RouterLinkActive } from "@angular/router"

interface SidebarItem {
  label: string
  icon: string
  route: string
}

@Component({
  selector: "app-sidebar",
  standalone: true,
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.scss"],
  imports: [
    RouterLink,
    RouterLinkActive
  ]
})
export class SidebarComponent {
  @Input() isCollapsed = false

  @ViewChild("asideEl", { read: ElementRef }) asideEl!: ElementRef<HTMLElement>

  sidebarItems: SidebarItem[] = [
    { label: "Dashboard", icon: "📊", route: "/admin/dashboard" },
    { label: "Phim", icon: "🎬", route: "/admin/movies" },
    { label: "Thể Loại", icon: "🏷️", route: "/admin/genres" },
    { label: "Studio", icon: "🎥", route: "/admin/studios" },
    { label: "Người Dùng", icon: "👥", route: "/admin/users" },
    { label: "Gói dịch vụ", icon: "💎", route: "/admin/plans" },
    { label: "Đơn Hàng", icon: "📦", route: "/admin/orders" },
    { label: "Thư Viện Media", icon: "🖼️", route: "/admin/media-library" },
    { label: "Cài Đặt", icon: "⚙️", route: "/admin/settings" },
    { label: "Nhật Ký Hoạt Động", icon: "📋", route: "/admin/audit-logs" },
  ]

  private router = inject(Router)

  startCollapsing() {
    const el = this.asideEl?.nativeElement;
    if (el && !el.classList.contains("collapsing")) {
      el.classList.add("collapsing");
    }
  }

  stopCollapsing() {
    const el = this.asideEl?.nativeElement;
    if (el && el.classList.contains("collapsing")) {
      el.classList.remove("collapsing");
    }
  }

  isActive(route: string): boolean {
    return this.router.url.includes(route)
  }
}
