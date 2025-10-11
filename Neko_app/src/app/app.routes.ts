import { Routes } from '@angular/router';
import { SigninComponent } from './components/signin/signin';
import { SignupComponent } from './components/signup/signup';
import { TextComponent } from './components/home/text/text'; // optional default text

export const routes: Routes = [
  {
    path: '',  // default layout
    component: TextComponent, // main layout
  },
  {
    path: 'signin',  // default layout
    component: SigninComponent, // main layout
  },
  {
    path: 'signup',  // default layout
    component: SignupComponent, // main layout
  },
  { path: '**', redirectTo: '' }
];
