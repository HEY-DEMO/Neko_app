// user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'http://localhost:5202/api/User'; // replace with your backend URL

  constructor(private http: HttpClient) { }

  // Checks if username, email, or mobile exists
  checkuser(data: { username: string; email: string; mobile: string }): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/checkuser?username=${data.username}&email=${data.email}&mobile=${data.mobile}`
    );
  }



  // Adds a new user
  adduser(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/signup`, data);
  }

  //user login
  login(emailOrMobile: string, password: string) {
    return this.http.post(`${this.apiUrl}/login`, { emailOrMobile, password });
  }
}
