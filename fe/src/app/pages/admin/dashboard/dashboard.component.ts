import {Component, inject, type OnInit} from "@angular/core"
import {AdminDashboardStats} from '../../../models/admin.model';
import {AdminService} from '../../../core/services/admin.service';
import {StatCardComponent} from '../../../components/admin/stat-card/stat-card.component';
import {ChartComponent} from '../../../components/admin/chart-placeholder/chart.component';
import {formatBytes} from '../../../lib/utils/byte.util';

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
  imports: [
    StatCardComponent,
    ChartComponent
  ]
})
export class DashboardComponent implements OnInit {
  stats: AdminDashboardStats | null = null
  isLoading = true

  private adminService = inject(AdminService);

  ngOnInit(): void {
    this.loadDashboardStats()
  }

  loadDashboardStats(): void {
    this.isLoading = true
    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data
        this.isLoading = false
      },
      error: () => {
        this.isLoading = false
      },
    })
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(value)
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat("vi-VN").format(value)
  }

  protected readonly formatBytes = formatBytes;
}
