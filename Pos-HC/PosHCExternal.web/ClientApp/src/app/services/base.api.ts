import { Injectable, inject } from '@angular/core';
import { LanguageService } from '../i18n/language';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BaseAPI {
  private language = inject(LanguageService);
  constructor(private http: HttpClient) {}
  private baseUrl = '/';

  private buildUrl(api: string): string {
    return `${this.baseUrl}${api}`;
  }
  // GET
  get<T>(api: string): Observable<T> {
    return this.http.get<T>(this.buildUrl(api));
  }

  // POST
  post<T>(api: string, body: any): Observable<T> {
    return this.http.post<T>(this.buildUrl(api), body);
  }

  // PUT
  put<T>(api: string, body: any): Observable<T> {
    return this.http.put<T>(this.buildUrl(api), body);
  }

  // DELETE
  delete<T>(api: string): Observable<T> {
    return this.http.delete<T>(this.buildUrl(api));
  }
  downloadPdf(api: string) {
    return this.http.get(this.buildUrl(api), {
      params: { language: this.language.language() },
      responseType: 'blob',
      observe: 'response',
    });
  }
}
