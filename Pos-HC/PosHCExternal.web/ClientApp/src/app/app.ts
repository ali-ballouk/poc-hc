import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  template: `
    <h1>CORS Demo</h1>
    <button (click)="callApi()">Call API</button>
    <p>{{ apiMessage }}</p>
  `
})
export class App implements OnInit {
  apiMessage = '';
  url = 'https://localhost:7076/api/doctor/lookup'

  constructor(private http: HttpClient) { }

  ngOnInit() { }

  callApi() {
    this.http.get<any>(this.url)
      .subscribe({
        next: (res) => this.apiMessage = res.message,
        error: (err) => this.apiMessage = 'Error: ' + err.message
      });
  }
}
