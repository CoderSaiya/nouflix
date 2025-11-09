import {Component, ViewChild} from "@angular/core"
import {HeaderComponent} from '../../components/admin/header/header.component';
import {RouterOutlet} from '@angular/router';
import {SidebarComponent} from '../../components/admin/sidebar/sidebar.component';

@Component({
  selector: "app-admin-layout",
  standalone: true,
  templateUrl: "./admin-layout.component.html",
  styleUrls: ["./admin-layout.component.scss"],
  imports: [
    HeaderComponent,
    RouterOutlet,
    SidebarComponent
  ]
})
export class AdminLayoutComponent {
  isSidebarCollapsed = false

  @ViewChild(SidebarComponent) sidebarCmp!: SidebarComponent;

  // timing
  private readonly COLLAPSE_DELAY_MS = 120;
  private readonly WIDTH_TRANSITION_MS = 320;
  private readonly TOTAL_MS = this.COLLAPSE_DELAY_MS + this.WIDTH_TRANSITION_MS + 80;

  toggleSidebar() {
    if (!this.sidebarCmp) {
      // fallback: toggle immediately
      this.isSidebarCollapsed = !this.isSidebarCollapsed;
      return;
    }

    this.sidebarCmp.startCollapsing();

    setTimeout(() => {
      this.isSidebarCollapsed = !this.isSidebarCollapsed;
    }, this.COLLAPSE_DELAY_MS);

    setTimeout(() => {
      this.sidebarCmp.stopCollapsing();
    }, this.TOTAL_MS);
  }
}
