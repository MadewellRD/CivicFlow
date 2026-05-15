import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { IconComponent } from './icon.component';
import { RoleSwitcherComponent } from './role-switcher.component';
import { CivicFlowApiService } from './civicflow-api.service';
import { UserContextService } from './user-context.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, IconComponent, RoleSwitcherComponent],
  template: `
    <div class="app-shell">
      <aside class="sidebar">
        <div class="sidebar-brand">
          <div class="logo">
            <svg viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 44 L22 32 L32 38 L42 22 L52 30" stroke="#F5A623" stroke-width="4" fill="none" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div class="stack">
            <span class="name">CivicFlow</span>
            <span class="sub">Budget Operations</span>
          </div>
        </div>

        <nav class="nav">
          <span class="sidebar-section-label">Workspace</span>
          <a class="nav-item" routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">
            <span class="nav-icon"><app-icon name="dashboard" [size]="18"></app-icon></span>
            Overview
          </a>
          <a class="nav-item" routerLink="/requests" routerLinkActive="active">
            <span class="nav-icon"><app-icon name="requests" [size]="18"></app-icon></span>
            Requests
          </a>
          <a class="nav-item" routerLink="/imports" routerLinkActive="active">
            <span class="nav-icon"><app-icon name="imports" [size]="18"></app-icon></span>
            Import Repair
          </a>
          <a class="nav-item" routerLink="/platform" routerLinkActive="active">
            <span class="nav-icon"><app-icon name="platform" [size]="18"></app-icon></span>
            Platform
          </a>
        </nav>

        <div class="sidebar-footer">
          <div>v0.2 · demo seed loaded</div>
          <div><a href="https://github.com/MadewellRD/CivicFlow" target="_blank" rel="noopener">View source</a></div>
          <div class="text-muted">Built for OFM · 2026</div>
        </div>
      </aside>

      <div class="stack">
        <header class="topbar">
          <div class="topbar-title">
            <span class="crumb">CivicFlow</span>
            <h1>{{ titleFromRoute() }}</h1>
          </div>
          <div class="topbar-actions">
            <app-role-switcher></app-role-switcher>
          </div>
        </header>
        <main class="main">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class AppComponent implements OnInit {
  constructor(private readonly api: CivicFlowApiService, private readonly ctx: UserContextService) {}

  ngOnInit(): void {
    this.ctx.activeUserId$.subscribe(id => {
      if (!id) {
        this.ctx.setCurrentUser(null);
        return;
      }

      this.api.me().subscribe({
        next: user => this.ctx.setCurrentUser(user),
        error: () => this.ctx.setCurrentUser(null),
      });
    });
  }

  titleFromRoute(): string {
    const p = (typeof location !== 'undefined' ? location.pathname : '/').toLowerCase();
    if (p.startsWith('/requests')) return 'Requests';
    if (p.startsWith('/imports')) return 'Data Integration Repair Center';
    if (p.startsWith('/platform')) return 'Platform Inspector';
    return 'Overview';
  }
}
