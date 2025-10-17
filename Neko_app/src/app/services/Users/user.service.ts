// user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'http://localhost:5202/api/User'; // your backend

  constructor(private http: HttpClient) { }

  // Existing methods
  checkuser(data: { username: string; email: string; mobile: string }): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/checkuser?username=${data.username}&email=${data.email}&mobile=${data.mobile}`
    );
  }

  // Signup user
  signup(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/signup`, data);
  }

  // --- New login method ---
  login(data: { emailOrMobile: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, data);
  }
}
