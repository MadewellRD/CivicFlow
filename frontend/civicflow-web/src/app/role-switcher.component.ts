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
      <label>
        Acting as
        <select [ngModel]="activeUserId" (ngModelChange)="onUserChange($event)">
          <option *ngFor="let user of users" [value]="user.id">
            {{ user.displayName }} &mdash; {{ user.primaryRole }}
          </option>
        </select>
      </label>
    </div>
  `,
  styles: [`
    .role-switcher select { margin-left: 0.5rem; }
    .role-switcher { display: inline-flex; align-items: center; font-size: 0.9rem; }
  `]
})
export class RoleSwitcherComponent implements OnInit {
  users: RosterUser[] = [];
  activeUserId: string | null = null;

  constructor(
    private readonly api: CivicFlowApiService,
    private readonly userContext: UserContextService
  ) {}

  ngOnInit(): void {
    this.api.listUsers().subscribe(users => {
      this.users = users;
      if (!this.userContext.currentUserId && users.length > 0) {
        this.userContext.setActiveUser(users[0].id);
      }
      this.activeUserId = this.userContext.currentUserId;
    });
  }

  onUserChange(userId: string): void {
    this.userContext.setActiveUser(userId);
    this.activeUserId = userId;
    // Soft refresh so views requery with the new identity
    location.reload();
  }
}
