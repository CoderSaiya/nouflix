import { CommonModule } from "@angular/common"
import { Component, inject, type OnInit } from "@angular/core"
import { FormsModule } from "@angular/forms"
import { AuditLog } from "../../../models/admin.model"
import { AdminService } from "../../../core/services/admin.service"

@Component({
  selector: "app-audit-logs",
  imports: [CommonModule, FormsModule],
  templateUrl: "./audit-logs.component.html",
  styleUrls: ["./audit-logs.component.scss"],
})
export class AuditLogsComponent implements OnInit {
  private adminSvc = inject(AdminService)

  auditLogs: AuditLog[] = []
  filteredLogs: AuditLog[] = []
  currentPage = 1
  itemsPerPage = 25
  searchTerm = ""
  selectedAction = "all"
  isLoading = true

  actionOptions = [
    { value: "all", label: "Tất Cả Hành Động" },

    { value: "create", label: "Tạo / Thêm Mới" },
    { value: "update", label: "Cập Nhật" },
    { value: "delete", label: "Xóa" },
    { value: "login", label: "Đăng Nhập" },
    { value: "logout", label: "Đăng Xuất" },
    { value: "get", label: "Lấy Thông Tin" },
    { value: "read", label: "Xem / Đọc Dữ Liệu" },
    { value: "access", label: "Truy Cập Tài Nguyên" },
    { value: "play", label: "Phát Nội Dung" },
    { value: "complete", label: "Hoàn Thành" },
    { value: "error", label: "Lỗi / Thất Bại" },
  ]

  ngOnInit(): void {
    this.loadAuditLogs()
  }

  loadAuditLogs(): void {
    this.isLoading = true
    setTimeout(() => {
      this.adminSvc
        .getAuditLogs(this.searchTerm, this.currentPage, this.itemsPerPage)
        .subscribe({
          next: (logs) => {
            this.auditLogs = logs
            this.filterLogs()
            this.isLoading = false
          },
          error: () => {
            this.isLoading = false
          },
        })
    }, 500)
  }

  filterLogs(): void {
    const search = this.searchTerm.toLowerCase().trim()

    this.filteredLogs = this.auditLogs.filter((log) => {
      const username = log.username?.toLowerCase() ?? ""
      const details = log.details?.toLowerCase() ?? ""
      const userId = log.userId?.toLowerCase() ?? ""

      const matchesSearch =
        !search ||
        username.includes(search) ||
        details.includes(search) ||
        userId.includes(search)

      const matchesAction =
        this.selectedAction === "all" || log.action === this.selectedAction

      return matchesSearch && matchesAction
    })
  }

  onSearchChange(): void {
    this.currentPage = 1
    this.filterLogs()
  }

  onActionFilterChange(): void {
    this.currentPage = 1
    this.filterLogs()
  }

  getActionBadgeClass(action: string): string {
    const classes: { [key: string]: string } = {
      create: "badge-success",
      update: "badge-info",
      delete: "badge-danger",
      login: "badge-primary",
      logout: "badge-secondary",
      get: "badge-info",
      read: "badge-info",
      access: "badge-warning",
      play: "badge-primary",
      complete: "badge-success",
      error: "badge-danger",
    }
    return classes[action] || "badge-secondary"
  }

  getActionLabel(action: string): string {
    const labels: { [key: string]: string } = {
      create: "Tạo / Thêm Mới",
      update: "Cập Nhật",
      delete: "Xóa",
      login: "Đăng Nhập",
      logout: "Đăng Xuất",
      get: "Lấy Thông Tin",
      read: "Xem / Đọc Dữ Liệu",
      access: "Truy Cập Tài Nguyên",
      play: "Phát Nội Dung",
      complete: "Hoàn Thành",
      error: "Lỗi / Thất Bại",
    }
    return labels[action] || action.toUpperCase()
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return "Không hợp lệ";
    return d.toLocaleString("vi-VN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit"
    });
  }

  getTimeAgo(dateStr: string): string {
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return "Không hợp lệ";

    const now = new Date();
    const diff = now.getTime() - d.getTime();

    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days > 0) return `${days} ngày trước`;
    if (hours > 0) return `${hours} giờ trước`;
    if (minutes > 0) return `${minutes} phút trước`;
    if (seconds > 5) return `${seconds} giây trước`;

    return "Vừa xong";
  }

  get paginatedLogs(): AuditLog[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage
    return this.filteredLogs.slice(startIndex, startIndex + this.itemsPerPage)
  }

  get totalPages(): number {
    return Math.ceil(this.filteredLogs.length / this.itemsPerPage)
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page
    }
  }
}
