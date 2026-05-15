import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { RosterUser } from './models';
import { UserContextService } from './user-context.service';

@Component({
  selector: 'app-role-switcher',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="role-switcher" *ngIf="users.length > 0">
      <div class="role-avatar">{{ initials(activeUser) }}</div>
      <div class="stack">
        <span class="label">Acting as</span>
        <select [ngModel]="activeUserId" (ngModelChange)="onUserChange($event)">
          <option *ngFor="let u of users" [value]="u.id">{{ u.displayName }} — {{ u.primaryRole }}</option>
        </select>
      </div>
    </div>
  `,
})
export class RoleSwitcherComponent implements OnInit {
  users: RosterUser[] = [];
  activeUserId: string | null = null;
  activeUser: RosterUser | null = null;
  constructor(private readonly api: CivicFlowApiService, private readonly ctx: UserContextService) {}
  ngOnInit(): void {
    this.api.listUsers().subscribe(users => {
      this.users = users;
      if (!this.ctx.currentUserId && users.length) this.ctx.setActiveUser(users[0].id);
      this.activeUserId = this.ctx.currentUserId;
      this.activeUser = users.find(u => u.id === this.activeUserId) ?? null;
    });
  }
  onUserChange(id: string) {
    this.ctx.setActiveUser(id);
    this.activeUserId = id;
    this.activeUser = this.users.find(u => u.id === id) ?? null;
  }
  initials(u: RosterUser | null): string {
    if (!u) return '';
    return u.displayName.split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase();
  }
}
