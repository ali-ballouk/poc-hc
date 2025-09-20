import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { RouterOutlet } from '@angular/router';
import { DoctorsService } from './services/doctors';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  doctorService = inject(DoctorsService);
  doctors = signal<any[]>([]);
  selected = signal<any>(null);
  devname = signal<string>('');
  object = this;
  ngOnDestroy(): void {
    console.log("out")
  }
  ngOnInit(): void {
    console.log("in");
    this.doctorService.getAllDoctorsInfo().subscribe((doctorlist) => {
      this.doctors.set(doctorlist);
    });
  }

}

