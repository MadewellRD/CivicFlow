import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { CivicRequest, CreateRequest } from './models';

@Component({
  selector: 'app-request-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h1>Request Dashboard</h1>
      <p>Submit and review budget, HR funding, finance data correction, and integration support requests.</p>
    </section>

    <section class="grid">
      <form class="card" (ngSubmit)="create()">
        <h2>New request</h2>
        <label>Title <input name="title" [(ngModel)]="draft.title" required /></label>
        <label>Category
          <select name="category" [(ngModel)]="draft.category">
            <option [ngValue]="0">Budget Change</option>
            <option [ngValue]="1">HR Funding Change</option>
            <option [ngValue]="2">Finance Data Correction</option>
            <option [ngValue]="3">Legacy Integration Issue</option>
          </select>
        </label>
        <label>Estimated amount <input name="amount" type="number" [(ngModel)]="draft.estimatedAmount" /></label>
        <label>Business justification <textarea name="justification" [(ngModel)]="draft.businessJustification" required></textarea></label>
        <button type="submit">Create request</button>
      </form>

      <div class="card">
        <h2>Open requests</h2>
        <button type="button" (click)="load()">Refresh</button>
        <article class="card" *ngFor="let request of requests">
          <strong>{{ request.requestNumber || 'Unnumbered' }}</strong>
          <h3>{{ request.title }}</h3>
          <span class="status">{{ request.status }}</span>
          <p>{{ request.businessJustification }}</p>
          <button type="button" (click)="submit(request)">Submit</button>
        </article>
      </div>
    </section>
  `
})
export class RequestDashboardComponent implements OnInit {
  readonly demoRequesterId = '10000000-0000-0000-0000-000000000001';
  readonly demoAgencyId = '20000000-0000-0000-0000-000000000001';
  requests: CivicRequest[] = [];
  draft: CreateRequest = {
    title: 'Agency budget data correction',
    category: 2,
    agencyId: this.demoAgencyId,
    requesterId: this.demoRequesterId,
    estimatedAmount: 1000,
    businessJustification: 'Correct a legacy fund code loaded from an agency import file.'
  };

  constructor(private readonly api: CivicFlowApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void { this.api.listRequests().subscribe(requests => this.requests = requests); }

  create(): void { this.api.createRequest(this.draft).subscribe(() => this.load()); }

  submit(request: CivicRequest): void { this.api.submitRequest(request.id, this.demoRequesterId).subscribe(() => this.load()); }
}
