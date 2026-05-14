import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { CivicRequest, CreateRequest, TriageRecommendation } from './models';
import { UserContextService } from './user-context.service';

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
            <option [ngValue]="4">Security/Access Change</option>
            <option [ngValue]="5">Reporting Request</option>
          </select>
        </label>
        <label>Estimated amount <input name="amount" type="number" [(ngModel)]="draft.estimatedAmount" /></label>
        <label>Business justification <textarea name="justification" [(ngModel)]="draft.businessJustification" required></textarea></label>
        <button type="submit">Create request</button>
      </form>

      <div class="card">
        <h2>Open requests</h2>
        <button type="button" (click)="load()">Refresh</button>
        <article class="card request-card" *ngFor="let request of requests">
          <header class="request-card-header">
            <strong>{{ request.requestNumber || 'Unnumbered' }}</strong>
            <span class="status status-{{ request.status | lowercase }}">{{ request.status }}</span>
          </header>
          <h3>{{ request.title }}</h3>
          <p class="justification">{{ request.businessJustification }}</p>
          <div class="actions">
            <button type="button" (click)="submit(request)" *ngIf="request.status === 'Draft'">Submit</button>
            <button
              type="button"
              (click)="recommendTriage(request.id)"
              [disabled]="triageInProgress.has(request.id)">
              {{ triageInProgress.has(request.id) ? 'Asking AI…' : 'AI triage' }}
            </button>
          </div>
          <div class="triage" *ngIf="triageFor(request.id) as rec">
            <header class="triage-header">
              <strong>AI triage recommendation</strong>
              <span>
                {{ rec.providerName }} &middot; {{ rec.confidence }} confidence &middot; {{ rec.latencyMs }}ms &middot; \${{ rec.estimatedCostUsd | number:'1.6-6' }}
                <span *ngIf="rec.servedFromMock"> &middot; mock</span>
                <span *ngIf="rec.servedFromKillSwitch"> &middot; kill-switch</span>
              </span>
            </header>
            <ul>
              <li><strong>Queue:</strong> {{ rec.recommendedQueue }}</li>
              <li><strong>Complexity:</strong> {{ rec.complexity }}</li>
              <li><strong>Human review required:</strong> {{ rec.humanReviewRequired ? 'yes' : 'no' }}</li>
              <li><strong>Rationale:</strong> {{ rec.rationale }}</li>
              <li *ngIf="rec.similarPastRequests.length">
                <strong>Similar past requests:</strong>
                <ul>
                  <li *ngFor="let s of rec.similarPastRequests">
                    {{ s.requestNumber }} &mdash; {{ s.title }} (similarity {{ s.similarityScore | number:'1.2-2' }})
                  </li>
                </ul>
              </li>
            </ul>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .request-card { margin-bottom: 0.5rem; }
    .request-card-header { display: flex; justify-content: space-between; align-items: baseline; }
    .status { font-size: 0.75rem; padding: 0.15rem 0.4rem; border-radius: 0.25rem; background: #e5e7eb; }
    .justification { color: #4b5563; font-size: 0.9rem; }
    .actions { display: flex; gap: 0.5rem; }
    .triage { margin-top: 0.5rem; padding: 0.5rem 0.75rem; border-left: 3px solid #10b981; background: #f0fdf4; font-size: 0.9rem; }
    .triage-header { display: flex; justify-content: space-between; font-size: 0.85rem; color: #065f46; margin-bottom: 0.25rem; }
  `]
})
export class RequestDashboardComponent implements OnInit {
  readonly demoAgencyId = '20000000-0000-0000-0000-000000000001';
  requests: CivicRequest[] = [];
  triageInProgress = new Set<string>();
  triageRecommendations = new Map<string, TriageRecommendation>();
  draft: CreateRequest = {
    title: 'Agency budget data correction',
    category: 2,
    agencyId: this.demoAgencyId,
    requesterId: '',
    estimatedAmount: 1000,
    businessJustification: 'Correct a legacy fund code loaded from an agency import file.'
  };

  constructor(
    private readonly api: CivicFlowApiService,
    private readonly userContext: UserContextService
  ) {}

  ngOnInit(): void {
    this.userContext.activeUserId$.subscribe(id => {
      this.draft.requesterId = id ?? '';
    });
    this.load();
  }

  load(): void {
    this.api.listRequests().subscribe(requests => this.requests = requests);
  }

  create(): void {
    if (!this.draft.requesterId) { return; }
    this.api.createRequest(this.draft).subscribe(() => this.load());
  }

  submit(request: CivicRequest): void {
    const userId = this.userContext.currentUserId;
    if (!userId) { return; }
    this.api.submitRequest(request.id, userId).subscribe(() => this.load());
  }

  recommendTriage(requestId: string): void {
    this.triageInProgress.add(requestId);
    this.api.recommendTriage(requestId).subscribe({
      next: rec => {
        this.triageRecommendations.set(requestId, rec);
        this.triageInProgress.delete(requestId);
      },
      error: () => this.triageInProgress.delete(requestId)
    });
  }

  triageFor(requestId: string): TriageRecommendation | undefined {
    return this.triageRecommendations.get(requestId);
  }
}
