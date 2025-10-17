import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  // Store JWT & refresh token in localStorage
  getJwtToken(): string | null {
    return localStorage.getItem('jwt');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  storeJwtToken(token: string) {
    localStorage.setItem('jwt', token);
  }

  storeRefreshToken(token: string) {
    localStorage.setItem('refreshToken', token);
  }

  login(payload: { emailOrMobile: string; password: string }): Observable<any> {
    return this.http.post('/api/User/login', payload);
  }

  refreshToken(refreshToken: string): Observable<any> {
    return this.http.post('/api/User/refresh', refreshToken);
  }

  logout() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('refreshToken');
    // optionally redirect to login page
  }
}
