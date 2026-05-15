import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type IconName =
  | 'dashboard'
  | 'requests'
  | 'imports'
  | 'platform'
  | 'sparkles'
  | 'triage'
  | 'upload'
  | 'refresh'
  | 'send'
  | 'arrowRight'
  | 'user'
  | 'check'
  | 'x'
  | 'shield'
  | 'bolt'
  | 'alert'
  | 'history';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg
      viewBox="0 0 24 24"
      [attr.width]="size"
      [attr.height]="size"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true">
      <ng-container [ngSwitch]="name">
        <ng-container *ngSwitchCase="'dashboard'">
          <rect x="3" y="3" width="7" height="9" />
          <rect x="14" y="3" width="7" height="5" />
          <rect x="14" y="12" width="7" height="9" />
          <rect x="3" y="16" width="7" height="5" />
        </ng-container>
        <ng-container *ngSwitchCase="'requests'">
          <path d="M9 11l3 3L22 4" />
          <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
        </ng-container>
        <ng-container *ngSwitchCase="'imports'">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
          <polyline points="7 10 12 15 17 10" />
          <line x1="12" y1="15" x2="12" y2="3" />
        </ng-container>
        <ng-container *ngSwitchCase="'platform'">
          <rect x="3" y="3" width="18" height="18" rx="2" />
          <line x1="9" y1="3" x2="9" y2="21" />
          <line x1="3" y1="9" x2="21" y2="9" />
        </ng-container>
        <ng-container *ngSwitchCase="'sparkles'">
          <path d="M12 2l2.4 6.4L21 11l-6.6 2.6L12 20l-2.4-6.4L3 11l6.6-2.6L12 2z" />
          <path d="M19 4l1 2 2 1-2 1-1 2-1-2-2-1 2-1 1-2z" />
        </ng-container>
        <ng-container *ngSwitchCase="'triage'">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <polyline points="22 4 12 14.01 9 11.01" />
        </ng-container>
        <ng-container *ngSwitchCase="'upload'">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
          <polyline points="17 8 12 3 7 8" />
          <line x1="12" y1="3" x2="12" y2="15" />
        </ng-container>
        <ng-container *ngSwitchCase="'refresh'">
          <polyline points="23 4 23 10 17 10" />
          <polyline points="1 20 1 14 7 14" />
          <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
        </ng-container>
        <ng-container *ngSwitchCase="'send'">
          <line x1="22" y1="2" x2="11" y2="13" />
          <polygon points="22 2 15 22 11 13 2 9 22 2" />
        </ng-container>
        <ng-container *ngSwitchCase="'arrowRight'">
          <line x1="5" y1="12" x2="19" y2="12" />
          <polyline points="12 5 19 12 12 19" />
        </ng-container>
        <ng-container *ngSwitchCase="'user'">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
          <circle cx="12" cy="7" r="4" />
        </ng-container>
        <ng-container *ngSwitchCase="'check'">
          <polyline points="20 6 9 17 4 12" />
        </ng-container>
        <ng-container *ngSwitchCase="'x'">
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </ng-container>
        <ng-container *ngSwitchCase="'shield'">
          <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
        </ng-container>
        <ng-container *ngSwitchCase="'bolt'">
          <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" />
        </ng-container>
        <ng-container *ngSwitchCase="'alert'">
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </ng-container>
        <ng-container *ngSwitchCase="'history'">
          <polyline points="1 4 1 10 7 10" />
          <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
        </ng-container>
      </ng-container>
    </svg>
  `,
})
export class IconComponent {
  @Input() name: IconName = 'dashboard';
  @Input() size = 16;
}
