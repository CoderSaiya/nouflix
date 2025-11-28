import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-movie-images',
  templateUrl: './movie-images.component.html',
  styleUrls: ['./movie-images.component.scss']
})
export class MovieImagesComponent {
  @Input() selectedPosters: File[] = [];
  @Input() selectedBackdrops: File[] = [];
  @Input() currentPosterUrl: string | null = null;
  @Input() currentBackdropUrl: string | null = null;
  @Output() postersSelected = new EventEmitter<File[]>();
  @Output() backdropsSelected = new EventEmitter<File[]>();
  @Output() removePoster = new EventEmitter<number>();
  @Output() removeBackdrop = new EventEmitter<number>();

  onPosterChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files) return;
    this.postersSelected.emit(Array.from(input.files));
  }
  onBackdropChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files) return;
    this.backdropsSelected.emit(Array.from(input.files));
  }
}
