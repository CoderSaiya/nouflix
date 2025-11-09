import { Component, Input } from "@angular/core"

@Component({
  selector: "app-stat-card",
  templateUrl: "./stat-card.component.html",
  styleUrls: ["./stat-card.component.scss"],
})
export class StatCardComponent {
  @Input() title = ""
  @Input() value = "0"
  @Input() icon = ""
  @Input() trend = "+0%"
}
