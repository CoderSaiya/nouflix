import {MovieItem} from '../../models/movie.model';
import {Component, Input, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';

@Component({
  selector: "movie-item",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./movie-items.component.html",
  styleUrls: ["./movie-items.component.scss"],
})
export class MovieItems implements OnInit {
  movieList: MovieItem[] = []

  @Input('list') set list(value: MovieItem[] | null | undefined) {
    this.movieList = value ?? [];
  }

  ngOnInit(): void {
  }

  getReleaseYear(date: string): number {
    return new Date(date).getFullYear()
  }
}
