import { Component, Input, Output, EventEmitter } from '@angular/core';
import {FormGroup, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {Genre, MovieType, Studio} from '../../../../models/movie.model';

@Component({
  selector: 'app-movie-info',
  templateUrl: './movie-info.component.html',
  styleUrls: ['./movie-info.component.scss'],
  imports: [
    FormsModule,
    ReactiveFormsModule
  ]
})
export class MovieInfoComponent {
  @Input() infoForm!: FormGroup;
  @Input() genres: Genre[] = [];
  @Input() studios: Studio[] = [];
  @Input() movieTypeEntries: [string,string][] = [];
  @Input() publishStatusEntries: [string,string][] = [];

  @Output() toggleGenre = new EventEmitter<number>();
  @Output() toggleStudio = new EventEmitter<number>();

  isGenreSelectedId(id: number) { const arr = this.infoForm.get('genres')?.value || []; return arr.includes(id); }
  isStudioSelectedId(id: number) { const arr = this.infoForm.get('studioIds')?.value || []; return arr.includes(id); }

  emitToggleGenre(id: number) { this.toggleGenre.emit(id); }
  emitToggleStudio(id: number) { this.toggleStudio.emit(id); }

  protected readonly MovieType = MovieType;
}
