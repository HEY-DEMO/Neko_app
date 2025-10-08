import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Observable } from 'rxjs';

interface User {
  id?: number;
  name: string;
  email: string;
  password: string;
  mobile: string;
}

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './user.html',
  styleUrls: ['./user.css']
})
export class UserComponent {
  users: User[] = [];
  newUser: User = { name: '', email: '', password: '', mobile: '' };
  message: string = '';

  private apiUrl = 'https://localhost:5202/api/user';

  constructor(private http: HttpClient) {
    this.loadUsers();
  }

  loadUsers(): void {
    this.http.get<User[]>(this.apiUrl).subscribe({
      next: (data: User[]) => this.users = data,
      error: (err: any) => console.error(err)
    });
  }

  addUser(): void {
    this.http.post<User>(this.apiUrl, this.newUser).subscribe({
      next: (user: any) => {
        this.message = 'User added successfully!';
        this.newUser = { name: '', email: '', password: '', mobile: '' };
        this.loadUsers();
      },
      error: (err: { error: string; }) => this.message = 'Error: ' + err.error
    });
  }
}
