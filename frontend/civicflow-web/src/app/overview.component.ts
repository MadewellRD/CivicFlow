import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CivicFlowApiService } from './civicflow-api.service';
import { CivicRequest } from './models';
import { StatusBadgeComponent } from './status-badge.component';
import { IconComponent } from './icon.component';

interface KpiSet {
  total: number;
  open: number;
  approved: number;
  closed: number;
  rejected: number;
  totalOpenValue: number;
  byStatus: Map<string, number>;
  byCategory: Map<string, number>;
}

@Component({
  selector: 'app-overview',
  standalone: true,
  imports: [CommonModule, RouterLink, StatusBadgeComponent, IconComponent],
  template: `
    <section class="stack gap-6">
      <!-- Hero -->
      <div class="card card-pad" style="background: linear-gradient(135deg, var(--navy-900), var(--navy-800)); color: var(--ice-200); border: 0;">
        <div class="row gap-6" style="justify-content: space-between; align-items: flex-start; flex-wrap: wrap;">
          <div class="stack gap-2" style="max-width: 640px;">
            <span class="pill pill-amber" style="background: rgba(245,166,35,0.2); color: var(--amber-500);">Portfolio demo · OFM IT Application Developer</span>
            <h1 style="color: var(--white); font-size: var(--text-3xl);">A .NET 8 budget operations platform with governed AI in the loop.</h1>
            <p style="color: var(--ice-300); font-size: var(--text-base); line-height: 1.6;">
              13-state workflow, 12 role-based authorization policies, ServiceNow-shape Business Rules and Transform Maps, schema-enforced AI features with cost telemetry and a kill-switch. Built end-to-end as a WOW project for the Olympia interview.
            </p>
            <div class="row gap-3 mt-2">
              <a routerLink="/requests" class="btn btn-accent">
                <app-icon name="arrowRight" [size]="14"></app-icon>
                Open the request board
              </a>
              <a routerLink="/imports" class="btn btn-ghost" style="background: rgba(202,220,252,0.1); color: var(--white); border-color: rgba(202,220,252,0.2);">
                Try AI error explainer
              </a>
            </div>
          </div>
          <div class="stack gap-2" style="min-width: 220px;">
            <span class="text-xs" style="color: var(--ice-300); text-transform: uppercase; letter-spacing: 0.08em;">Stack</span>
            <div class="row gap-2" style="flex-wrap: wrap;">
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">.NET 8</span>
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">EF Core</span>
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">SQL Server</span>
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">Angular 18</span>
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">T-SQL</span>
              <span class="pill" style="background: rgba(255,255,255,0.1); color: var(--white);">Docker</span>
            </div>
          </div>
        </div>
      </div>

      <!-- KPI tiles -->
      <div class="grid grid-4 gap-4">
        <div class="kpi">
          <span class="kpi-label">Total requests</span>
          <span class="kpi-value">{{ kpi.total }}</span>
          <span class="kpi-trend">across {{ kpi.byCategory.size }} categories</span>
        </div>
        <div class="kpi kpi-amber">
          <span class="kpi-label">Open</span>
          <span class="kpi-value">{{ kpi.open }}</span>
          <span class="kpi-trend">{{ '$' + (formatCurrencyShort(kpi.totalOpenValue)) }} estimated value</span>
        </div>
        <div class="kpi kpi-green">
          <span class="kpi-label">Approved</span>
          <span class="kpi-value">{{ kpi.approved }}</span>
          <span class="kpi-trend">ready for implementation</span>
        </div>
        <div class="kpi kpi-blue">
          <span class="kpi-label">Closed</span>
          <span class="kpi-value">{{ kpi.closed }}</span>
          <span class="kpi-trend">{{ kpi.rejected }} rejected</span>
        </div>
      </div>

      <!-- Two-column lower section -->
      <div class="grid grid-2 gap-6">
        <!-- Recent requests -->
        <div class="card">
          <div class="card-header">
            <h2>Recent activity</h2>
            <a routerLink="/requests" class="btn btn-ghost btn-sm">
              View all <app-icon name="arrowRight" [size]="12"></app-icon>
            </a>
          </div>
          <table class="table">
            <thead>
              <tr><th>Request</th><th>Title</th><th>Status</th><th></th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let r of recent">
                <td><span class="request-number">{{ r.requestNumber || '—' }}</span></td>
                <td>
                  <div class="fw-semibold">{{ r.title }}</div>
                  <div class="text-xs text-muted">{{ humanize(r.category) }}</div>
                </td>
                <td><app-status-badge [status]="r.status"></app-status-badge></td>
                <td style="text-align: right;">
                  <a routerLink="/requests" [queryParams]="{ status: r.status }" class="btn btn-ghost btn-sm" title="Open filtered request board">
                    <app-icon name="arrowRight" [size]="12"></app-icon>
                  </a>
                </td>
              </tr>
              <tr *ngIf="!recent.length"><td colspan="4" class="text-muted" style="text-align:center;padding:1.5rem;">No requests yet.</td></tr>
            </tbody>
          </table>
        </div>

        <!-- Status distribution -->
        <div class="card">
          <div class="card-header">
            <h2>By status</h2>
            <span class="text-xs text-muted">{{ kpi.total }} total</span>
          </div>
          <div class="card-pad">
            <div class="stack gap-3">
              <div *ngFor="let row of statusRows()" class="stack gap-1">
                <div class="row" style="justify-content: space-between;">
                  <app-status-badge [status]="row.status"></app-status-badge>
                  <span class="text-xs fw-semibold text-navy">{{ row.count }}</span>
                </div>
                <div style="height: 6px; background: var(--ink-50); border-radius: 999px; overflow: hidden;">
                  <div [style.width.%]="kpi.total ? (row.count / kpi.total) * 100 : 0"
                       [style.background]="statusColor(row.status)"
                       style="height: 100%; transition: width var(--t-med) var(--ease);"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Why this matters -->
      <div class="grid grid-3 gap-4">
        <div class="card card-pad stack gap-3">
          <div class="row gap-2" style="color: var(--navy-800);">
            <app-icon name="sparkles" [size]="18"></app-icon>
            <h3 style="font-size: var(--text-base);">AI in the loop, governed</h3>
          </div>
          <p class="text-sm text-muted">Schema-enforced output, per-call cost telemetry, kill-switch decorator. One flag short-circuits every model call with a logged audit event.</p>
          <a routerLink="/requests" class="text-sm fw-semibold text-navy">Try the triage router →</a>
        </div>
        <div class="card card-pad stack gap-3">
          <div class="row gap-2" style="color: var(--navy-800);">
            <app-icon name="platform" [size]="18"></app-icon>
            <h3 style="font-size: var(--text-base);">ServiceNow-shape platform</h3>
          </div>
          <p class="text-sm text-muted">Business Rule engine with Before/After/Async phases, Transform Maps, UI Policies. Every concept named after its ServiceNow analogue.</p>
          <a routerLink="/platform" class="text-sm fw-semibold text-navy">Inspect the platform →</a>
        </div>
        <div class="card card-pad stack gap-3">
          <div class="row gap-2" style="color: var(--navy-800);">
            <app-icon name="shield" [size]="18"></app-icon>
            <h3 style="font-size: var(--text-base);">Role-based, audited</h3>
          </div>
          <p class="text-sm text-muted">12 named authorization policies. Every endpoint gated, every status transition audited. Use the role switcher in the top right to see actions change.</p>
          <span class="text-sm fw-semibold text-navy">Try switching to Read-Only Auditor →</span>
        </div>
      </div>
    </section>
  `,
})
export class OverviewComponent implements OnInit {
  recent: CivicRequest[] = [];
  kpi: KpiSet = { total: 0, open: 0, approved: 0, closed: 0, rejected: 0, totalOpenValue: 0, byStatus: new Map(), byCategory: new Map() };
  constructor(private readonly api: CivicFlowApiService) {}
  ngOnInit(): void {
    this.api.listRequests().subscribe(requests => {
      this.recent = [...requests]
        .sort((a, b) => (b.submittedAt ?? '').localeCompare(a.submittedAt ?? ''))
        .slice(0, 8);
      this.computeKpis(requests);
    });
  }
  computeKpis(requests: CivicRequest[]) {
    const open = new Set(['Draft','Submitted','Triage','AnalystReview','TechnicalReview','Approved','Blocked','ReturnedForCorrection','Reopened']);
    const byStatus = new Map<string, number>();
    const byCategory = new Map<string, number>();
    let approved = 0, closed = 0, rejected = 0, totalOpen = 0, totalOpenValue = 0;
    for (const r of requests) {
      byStatus.set(r.status, (byStatus.get(r.status) ?? 0) + 1);
      byCategory.set(r.category, (byCategory.get(r.category) ?? 0) + 1);
      if (r.status === 'Approved') approved++;
      if (r.status === 'Closed') closed++;
      if (r.status === 'Rejected') rejected++;
      if (open.has(r.status)) { totalOpen++; totalOpenValue += r.estimatedAmount ?? 0; }
    }
    this.kpi = { total: requests.length, open: totalOpen, approved, closed, rejected, totalOpenValue, byStatus, byCategory };
  }
  statusRows(): { status: string; count: number }[] {
    return Array.from(this.kpi.byStatus.entries())
      .map(([status, count]) => ({ status, count }))
      .sort((a, b) => b.count - a.count);
  }
  statusColor(status: string): string {
    const map: Record<string, string> = {
      Draft: 'var(--ink-400)', Submitted: 'var(--blue-500)', Triage: 'var(--purple-500)',
      AnalystReview: 'var(--purple-500)', TechnicalReview: 'var(--navy-700)',
      Approved: 'var(--green-500)', Implemented: 'var(--green-600)', Closed: 'var(--ink-300)',
      Rejected: 'var(--red-500)', Cancelled: 'var(--ink-300)', Blocked: 'var(--orange-500)',
      Reopened: 'var(--amber-500)', ReturnedForCorrection: 'var(--orange-500)',
    };
    return map[status] ?? 'var(--ink-300)';
  }
  humanize(s: string): string { return s.replace(/([a-z])([A-Z])/g, '$1 $2'); }
  formatCurrencyShort(n: number): string {
    if (n >= 1e6) return (n / 1e6).toFixed(1) + 'M';
    if (n >= 1e3) return (n / 1e3).toFixed(1) + 'K';
    return n.toFixed(0);
  }
}
