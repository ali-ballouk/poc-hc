import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { PosHcEditor } from './pos-hc-editor/pos-hc-editor';
import { RouterOutlet } from '@angular/router';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, PosHcEditor],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  apiMessage = '';
  baseurl = 'http://localhost:5269/'


  ngOnInit() { }

  //callApi() {
  //  let api : string = "api/doctor/lookup";
  //  this.http.get<any>(this.baseurl+api)
  //    .subscribe({
  //      next: (res) => console.log(res),
  //      error: (err) => this.apiMessage = 'Error: ' + err.message
  //    });
  //}
}
