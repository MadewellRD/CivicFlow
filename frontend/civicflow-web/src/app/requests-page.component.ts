import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { CivicRequest, CreateRequest, TriageRecommendation, RosterUser } from './models';
import { UserContextService } from './user-context.service';
import { StatusBadgeComponent } from './status-badge.component';
import { IconComponent } from './icon.component';

@Component({
  selector: 'app-requests-page',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusBadgeComponent, IconComponent],
  template: `
    <section class="stack gap-6">
      <!-- Toolbar -->
      <div class="row gap-3" style="justify-content: space-between; flex-wrap: wrap;">
        <div class="stack gap-1">
          <span class="text-xs text-muted" style="text-transform: uppercase; letter-spacing: 0.06em; font-weight: 600;">{{ requests.length }} requests</span>
          <h1>Request board</h1>
        </div>
        <div class="row gap-2" style="flex-wrap: wrap;">
          <select class="select" [(ngModel)]="filterStatus" style="min-width: 160px;">
            <option value="">All statuses</option>
            <option *ngFor="let s of allStatuses" [value]="s">{{ humanize(s) }}</option>
          </select>
          <button type="button" class="btn btn-ghost" (click)="load()">
            <app-icon name="refresh" [size]="14"></app-icon> Refresh
          </button>
          <button type="button" class="btn btn-primary" (click)="creating = !creating">
            <app-icon name="send" [size]="14"></app-icon> New request
          </button>
        </div>
      </div>

      <!-- New request form (collapsible) -->
      <div class="card" *ngIf="creating">
        <div class="card-header">
          <h2>Create a new request</h2>
          <button type="button" class="btn btn-ghost btn-sm" (click)="creating = false"><app-icon name="x" [size]="12"></app-icon></button>
        </div>
        <form class="card-pad stack gap-4" (ngSubmit)="create()">
          <div class="grid grid-2 gap-4">
            <div class="field">
              <label>Title</label>
              <input class="input" name="title" [(ngModel)]="draft.title" required />
            </div>
            <div class="field">
              <label>Category</label>
              <select class="select" name="category" [(ngModel)]="draft.category">
                <option [ngValue]="0">Budget Change</option>
                <option [ngValue]="1">HR Funding Change</option>
                <option [ngValue]="2">Finance Data Correction</option>
                <option [ngValue]="3">Legacy Integration Issue</option>
                <option [ngValue]="4">Security / Access Change</option>
                <option [ngValue]="5">Reporting Request</option>
              </select>
            </div>
            <div class="field">
              <label>Estimated amount (USD)</label>
              <input class="input" name="amount" type="number" [(ngModel)]="draft.estimatedAmount" />
            </div>
            <div class="field">
              <label>Acting as</label>
              <input class="input" name="actor" [value]="currentActorName" disabled />
            </div>
          </div>
          <div class="field">
            <label>Business justification</label>
            <textarea class="textarea" name="justification" [(ngModel)]="draft.businessJustification" required></textarea>
          </div>
          <div class="row gap-2">
            <button type="submit" class="btn btn-primary" [disabled]="!draft.title || !draft.businessJustification || !draft.requesterId">
              <app-icon name="send" [size]="14"></app-icon> Create request
            </button>
            <button type="button" class="btn btn-ghost" (click)="creating = false">Cancel</button>
          </div>
        </form>
      </div>

      <!-- Request list -->
      <div class="stack gap-3">
        <article *ngFor="let r of filteredRequests()" class="card card-pad stack gap-3">
          <div class="row gap-3" style="justify-content: space-between; align-items: flex-start; flex-wrap: wrap;">
            <div class="stack gap-1" style="min-width: 0; flex: 1;">
              <div class="row gap-2" style="align-items: baseline;">
                <span class="request-number">{{ r.requestNumber || 'CF-DRAFT' }}</span>
                <app-status-badge [status]="r.status"></app-status-badge>
                <span class="pill">{{ humanize(r.category) }}</span>
              </div>
              <h3 style="font-size: var(--text-base); color: var(--navy-800);">{{ r.title }}</h3>
              <p class="text-sm text-muted" style="max-width: 760px;">{{ r.businessJustification }}</p>
              <div class="row gap-3 mt-2 text-xs text-muted">
                <span><strong>Amount:</strong> {{ '$' + ((r.estimatedAmount || 0).toLocaleString()) }}</span>
                <span *ngIf="r.submittedAt"><strong>Submitted:</strong> {{ relative(r.submittedAt) }}</span>
              </div>
            </div>
            <div class="row gap-2" style="flex-shrink: 0;">
              <button type="button" class="btn btn-ghost btn-sm" *ngIf="r.status === 'Draft'" (click)="submit(r)">
                <app-icon name="send" [size]="12"></app-icon> Submit
              </button>
              <button type="button" class="btn btn-ai btn-sm" (click)="recommend(r.id)" [disabled]="busy.has(r.id)">
                <app-icon name="sparkles" [size]="12"></app-icon>
                <span *ngIf="!busy.has(r.id)">AI triage</span>
                <span *ngIf="busy.has(r.id)" class="row gap-2"><span class="spinner"></span> Thinking…</span>
              </button>
            </div>
          </div>

          <!-- AI triage panel -->
          <div *ngIf="triage.get(r.id) as t" class="ai-panel triage">
            <div class="ai-panel-header">
              <div class="title"><app-icon name="triage" [size]="14"></app-icon> AI triage recommendation</div>
              <div class="ai-panel-meta">
                <span [ngClass]="t.servedFromMock ? 'badge badge-mock' : t.servedFromKillSwitch ? 'badge badge-killswitch' : 'badge badge-live'">
                  {{ t.servedFromKillSwitch ? 'kill-switch' : t.servedFromMock ? 'mock' : 'live · ' + t.providerName }}
                </span>
                <span>{{ t.confidence }} conf.</span>
                <span>{{ t.latencyMs }}ms</span>
                <span>{{ '$' + (t.estimatedCostUsd | number:'1.6-6') }}</span>
              </div>
            </div>
            <div class="ai-panel-body">
              <div class="grid grid-3 gap-3">
                <div class="stack gap-1">
                  <span class="text-xs text-muted fw-semibold">Recommended queue</span>
                  <span class="fw-bold text-navy">{{ t.recommendedQueue }}</span>
                </div>
                <div class="stack gap-1">
                  <span class="text-xs text-muted fw-semibold">Complexity</span>
                  <span class="pill" [ngClass]="{ 'pill-green': t.complexity==='low', 'pill-amber': t.complexity==='medium', 'pill-blue': t.complexity==='high' }">{{ t.complexity }}</span>
                </div>
                <div class="stack gap-1">
                  <span class="text-xs text-muted fw-semibold">Human review</span>
                  <span class="pill" [ngClass]="t.humanReviewRequired ? 'pill-amber' : 'pill-green'">
                    {{ t.humanReviewRequired ? 'Required' : 'Not required' }}
                  </span>
                </div>
              </div>
              <p class="summary">{{ t.rationale }}</p>
              <div class="similar-list" *ngIf="t.similarPastRequests?.length">
                <span class="text-xs text-muted fw-semibold">Similar past requests</span>
                <div *ngFor="let s of t.similarPastRequests" class="similar-row">
                  <span class="text-sm"><span class="request-number">{{ s.requestNumber }}</span> · {{ s.title }}</span>
                  <span class="similarity-score">{{ s.similarityScore | number:'1.2-2' }}</span>
                </div>
              </div>
            </div>
          </div>
        </article>

        <div *ngIf="!requests.length" class="card empty-state">
          <app-icon name="requests" [size]="32"></app-icon>
          <h3>No requests yet</h3>
          <p class="text-sm text-muted">Create one using the New request button above.</p>
        </div>
      </div>
    </section>
  `,
})
export class RequestsPageComponent implements OnInit {
  requests: CivicRequest[] = [];
  busy = new Set<string>();
  triage = new Map<string, TriageRecommendation>();
  creating = false;
  filterStatus = '';
  currentActorName = '';
  allStatuses = ['Draft','Submitted','Triage','AnalystReview','TechnicalReview','Approved','Implemented','Closed','Rejected','Cancelled','Blocked','Reopened','ReturnedForCorrection'];

  draft: CreateRequest = {
    title: 'Q4 forecast adjustment for caseload growth',
    category: 0,
    agencyId: '20000000-0000-0000-0000-000000000001',
    requesterId: '',
    estimatedAmount: 250000,
    businessJustification: 'Revised caseload projections require a supplemental allotment.'
  };

  constructor(private readonly api: CivicFlowApiService, private readonly ctx: UserContextService) {}

  ngOnInit(): void {
    this.ctx.activeUserId$.subscribe(id => { this.draft.requesterId = id ?? ''; });
    this.api.listUsers().subscribe(users => {
      const me = users.find(u => u.id === this.ctx.currentUserId);
      this.currentActorName = me ? `${me.displayName} (${me.primaryRole})` : '';
    });
    this.load();
  }

  load(): void { this.api.listRequests().subscribe(r => this.requests = r); }

  filteredRequests(): CivicRequest[] {
    if (!this.filterStatus) return this.requests;
    return this.requests.filter(r => r.status === this.filterStatus);
  }

  create(): void {
    if (!this.draft.requesterId) return;
    this.api.createRequest(this.draft).subscribe(() => {
      this.creating = false;
      this.load();
    });
  }

  submit(r: CivicRequest): void {
    const id = this.ctx.currentUserId;
    if (!id) return;
    this.api.submitRequest(r.id, id).subscribe(() => this.load());
  }

  recommend(id: string): void {
    this.busy.add(id);
    this.api.recommendTriage(id).subscribe({
      next: rec => { this.triage.set(id, rec); this.busy.delete(id); },
      error: () => this.busy.delete(id),
    });
  }

  humanize(s: string): string { return s.replace(/([a-z])([A-Z])/g, '$1 $2'); }

  relative(iso: string): string {
    const t = new Date(iso).getTime();
    if (isNaN(t)) return iso;
    const diff = Date.now() - t;
    const days = Math.floor(diff / 86400000);
    if (days < 1) return 'today';
    if (days === 1) return 'yesterday';
    if (days < 30) return `${days} days ago`;
    return new Date(iso).toLocaleDateString();
  }
}
