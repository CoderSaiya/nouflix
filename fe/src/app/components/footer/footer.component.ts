import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterLink } from "@angular/router"

@Component({
  selector: "app-footer",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./footer.component.html",
  styleUrls: ["./footer.component.scss"],
})
export class FooterComponent {
  currentYear = new Date().getFullYear()

  footerLinks = {
    company: [
      { label: "Giới Thiệu", url: "/about" },
      { label: "Liên Hệ", url: "/contact" },
      { label: "Tuyển Dụng", url: "/careers" },
      { label: "Tin Tức", url: "/news" },
    ],
    support: [
      { label: "Trung Tâm Trợ Giúp", url: "/help" },
      { label: "Điều Khoản Sử Dụng", url: "/terms" },
      { label: "Chính Sách Bảo Mật", url: "/privacy" },
      { label: "FAQ", url: "/faq" },
    ],
    social: [
      { label: "Facebook", url: "https://facebook.com", icon: "facebook" },
      { label: "Twitter", url: "https://twitter.com", icon: "twitter" },
      { label: "Instagram", url: "https://instagram.com", icon: "instagram" },
      { label: "YouTube", url: "https://youtube.com", icon: "youtube" },
    ],
  }
}
