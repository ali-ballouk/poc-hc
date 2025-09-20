
import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core/primitives/di';
import { catchError } from 'rxjs/internal/operators/catchError';
import { of } from 'rxjs/internal/observable/of';
import { tap, throwError } from 'rxjs';


@Injectable({
  providedIn: 'root'
})

export class DoctorsService {


  http = inject(HttpClient);
  getAllDoctorsInfo() {
    let url = '/api/doctor/lookup';
    return this.http.get<any[]>(url).pipe(catchError((err: any) => {
      console.error('Load users failed:', err);
      return of([]); // fallback value keeps the stream alive
    }))
  }

}
