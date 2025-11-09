import { Component, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-movie-subtitles',
  templateUrl: './movie-subtitles.component.html',
  styleUrls: ['./movie-subtitles.component.scss']
})
export class MovieSubtitlesComponent {
  @Output() subtitlesSelected = new EventEmitter<FileList>();
  onSubtitleChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files) return;
    this.subtitlesSelected.emit(input.files);
    input.value = '';
  }
}
