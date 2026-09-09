import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './login.component.html',
})
export class LoginComponent {
  auth = inject(AuthService);
  http = inject(HttpClient);
  router = inject(Router);
  username = '';
  password = '';
  displayName = '';
  token = '';
  reset = false;
  busy = false;
  error = '';
  submit() {
    this.busy = true;
    this.error = '';
    const endpoint = this.auth.setupRequired()
      ? 'setup'
      : this.reset
        ? 'reset'
        : 'login';
    this.http
      .post('/api/auth/' + endpoint, {
        Username: this.username,
        Password: this.password,
        DisplayName: this.displayName,
        Token: this.token,
      })
      .subscribe({
        next: () => {
          this.busy = false;
          this.password = '';
          this.token = '';
          this.reset = false;
          this.auth.load().subscribe(() => {
            if (this.auth.user())
              this.router.navigateByUrl(
                this.auth.can('Doctor')
                  ? '/manage/appointments'
                  : this.auth.can('Cashier')
                    ? '/billing'
                    : '/pointofsale',
              );
          });
        },
        error: (e) => {
          this.busy = false;
          this.error =
            e.error?.detail ||
            'Sign in failed. Check your credentials and try again.';
        },
      });
  }
}
