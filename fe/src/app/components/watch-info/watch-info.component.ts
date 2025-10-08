import {Component, Input, OnInit} from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterLink } from "@angular/router"
import type { Movie } from "../../models/movie.model"

@Component({
  selector: "app-watch-info",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./watch-info.component.html",
  styleUrl: "./watch-info.component.scss",
})
export class WatchInfoComponent implements OnInit {
  @Input() movie!: Movie

  isExpanded = false

  toggleDescription(): void {
    this.isExpanded = !this.isExpanded
  }

  formatRuntime(minutes: number): string {
    const hours = Math.floor(minutes / 60)
    const mins = minutes % 60
    return `${hours}h ${mins}m`
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
    }).format(amount)
  }

  ngOnInit(): void {
  }
}
