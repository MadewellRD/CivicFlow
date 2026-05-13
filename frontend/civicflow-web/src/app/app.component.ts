import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header>
      <strong>CivicFlow Budget Operations</strong>
      <nav>
        <a routerLink="/">Requests</a>
        <a routerLink="/imports">Import Repair Center</a>
      </nav>
    </header>
    <main><router-outlet /></main>
  `
})
export class AppComponent {}
