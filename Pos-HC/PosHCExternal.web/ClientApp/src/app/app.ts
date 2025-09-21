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
  baseurl = 'http://localhost:5269/'

  constructor(private http: HttpClient) { }

  ngOnInit() { }

  callApi() {
    let api : string = "api/doctor/lookup";
    this.http.get<any>(this.baseurl+api)
      .subscribe({
        next: (res) => console.log(res),
        error: (err) => this.apiMessage = 'Error: ' + err.message
      });
  }
}
