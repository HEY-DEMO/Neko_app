import { Component } from '@angular/core';
import { UserComponent } from './components/user/user';
import { NavbarComponent } from './components/navbar/navbar'
import { HomeComponent } from './components/home/home';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UserComponent,NavbarComponent,HomeComponent],
  template: `<app-home></app-home>`,
})
export class AppComponent { }
