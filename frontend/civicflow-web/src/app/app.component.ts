import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { RoleSwitcherComponent } from './role-switcher.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RoleSwitcherComponent],
  template: `
    <header>
      <strong>CivicFlow Budget Operations</strong>
      <nav>
        <a routerLink="/">Requests</a>
        <a routerLink="/imports">Import Repair Center</a>
      </nav>
      <app-role-switcher></app-role-switcher>
    </header>
    <main><router-outlet /></main>
  `
})
export class AppComponent {}
