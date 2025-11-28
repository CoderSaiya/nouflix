import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Subtitle } from '../../../../models/movie.model';

@Component({
  selector: 'app-movie-subtitles',
  templateUrl: './movie-subtitles.component.html',
  styleUrls: ['./movie-subtitles.component.scss']
})
export class MovieSubtitlesComponent {
  @Input() subtitles: Subtitle[] = [];
  @Output() subtitlesSelected = new EventEmitter<FileList>();
  @Output() deleteSubtitle = new EventEmitter<number>();

  onSubtitleChange(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files) return;
    this.subtitlesSelected.emit(input.files);
    input.value = '';
  }
}
