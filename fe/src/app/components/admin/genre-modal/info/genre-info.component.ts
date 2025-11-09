import { Component, Input, Output, EventEmitter } from '@angular/core';
import {FormGroup, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MovieType} from '../../../../models/movie.model';

@Component({
  selector: 'app-genre-info',
  templateUrl: './genre-info.component.html',
  styleUrls: ['./genre-info.component.scss'],
  imports: [
    FormsModule,
    ReactiveFormsModule
  ]
})
export class GenreInfoComponent {
  @Input() infoForm!: FormGroup;

  protected readonly MovieType = MovieType;
}
