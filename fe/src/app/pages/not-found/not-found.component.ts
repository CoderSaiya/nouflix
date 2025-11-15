import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss'
})
export class NotFoundComponent {
  private route = inject(ActivatedRoute)
  private router = inject(Router)

  onGoBack(): void {
    this.route.params.subscribe((params) => {
      const returnUrl = String(params["returnUrl"])
      this.router.navigate([returnUrl || "/"])
    })
  }
}
