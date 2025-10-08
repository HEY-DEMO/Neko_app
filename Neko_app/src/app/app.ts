import { Component } from '@angular/core';
import { UserComponent } from './components/user/user';
import { NavbarComponent } from './components/navbar/navbar'

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UserComponent,NavbarComponent],
  template: `<app-navbar></app-navbar><app-user></app-user>`,
})
export class AppComponent { }
