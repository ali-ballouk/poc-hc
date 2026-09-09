import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpInterceptorFn } from '@angular/common/http';
import { catchError, tap, throwError } from 'rxjs';
export interface StaffSession {
  Username: string;
  Role: string;
}
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  user = signal<StaffSession | null>(null);
  ready = signal(false);
  setupRequired = signal(false);
  error = signal('');
  load() {
    return this.http
      .get<{
        User: StaffSession | null;
        SetupRequired: boolean;
      }>('/api/auth/session')
      .pipe(
        tap((s) => {
          this.user.set(s.User);
          this.setupRequired.set(s.SetupRequired);
          this.ready.set(true);
        }),
      );
  }
  initialize() {
    this.load().subscribe({
      error: () => {
        this.error.set(
          'Unable to connect. Check that the API and database are running.',
        );
        this.ready.set(true);
      },
    });
  }
  can(...roles: string[]) {
    return !!this.user() && roles.includes(this.user()!.Role);
  }
  canRoles(roles: string[]) {
    return this.can(...roles);
  }
  logout() {
    this.http.post('/api/auth/logout', {}).subscribe(() => {
      this.user.set(null);
      this.initialize();
    });
  }
}
export const sessionInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  return next(request).pipe(
    catchError((error) => {
      if (error.status === 401 && auth.user()) {
        auth.user.set(null);
        auth.initialize();
      }
      return throwError(() => error);
    }),
  );
};
