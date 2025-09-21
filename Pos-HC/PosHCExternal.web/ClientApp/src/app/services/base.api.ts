import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BaseAPI {

  constructor(private http: HttpClient) { }
  private baseUrl = 'http://localhost:5269/';

  private buildUrl(api: string): string {
    return `${this.baseUrl}${api}`;
  }
  // GET
  get<T>(api: string): Observable<T> {
    return this.http.get<T>(this.buildUrl(api));
  }

  // POST
  post<T>(api: string, body: any): Observable<T> {
    return this.http.post<T>(this.buildUrl(api),body);
  }

  // PUT
  put<T>(api: string, body: any): Observable<T> {
    return this.http.put<T>(this.buildUrl(api), body);
  }

  // DELETE
  delete<T>(api: string): Observable<T> {
    return this.http.delete<T>(this.buildUrl(api));
  }
}
