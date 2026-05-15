import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge badge-{{ status }}">{{ humanize(status) }}</span>`,
})
export class StatusBadgeComponent {
  @Input() status: string = '';
  humanize(s: string): string {
    return s.replace(/([a-z])([A-Z])/g, '$1 $2');
  }
}
