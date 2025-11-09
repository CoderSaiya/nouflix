import { Component, Input } from '@angular/core';
import {FormGroup, FormsModule, ReactiveFormsModule} from '@angular/forms';

@Component({
  selector: 'app-studio-info',
  templateUrl: './studio-info.component.html',
  styleUrls: ['./studio-info.component.scss'],
  imports: [
    FormsModule,
    ReactiveFormsModule
  ]
})
export class StudioInfoComponent {
  @Input() infoForm!: FormGroup;
}
