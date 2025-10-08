import { Component, Input } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterLink } from "@angular/router"
import type { Movie } from "../../models/movie.model"

@Component({
  selector: "app-recommendations",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./recommendations.component.html",
  styleUrl: "./recommendations.component.scss",
})
export class RecommendationsComponent {
  @Input() movies: Movie[] = []
}
