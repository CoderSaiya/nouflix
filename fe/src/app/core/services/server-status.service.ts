import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class ServerStatusService {
  private lastShownAt = 0;

  constructor(private snack: MatSnackBar) {}

  show(message: string) {
    const now = Date.now();
    // chặn spam: nếu trong 10s đã show rồi thì bỏ qua
    if (now - this.lastShownAt < 10_000) return;
    this.lastShownAt = now;

    this.snack.open(message, 'Đóng', {
      duration: 5000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}
