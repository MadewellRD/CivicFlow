import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { ImportBatchSummary, ImportErrorExplanationBatch, ImportRow } from './models';
import { UserContextService } from './user-context.service';
import { IconComponent } from './icon.component';

@Component({
  selector: 'app-import-repair-center',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  template: `
    <section class="stack gap-6">
      <div class="card card-pad stack gap-4">
        <div class="row gap-4" style="justify-content: space-between; align-items: flex-start; flex-wrap: wrap;">
          <div class="stack gap-2" style="max-width: 720px;">
            <h1>Data Integration Repair Center</h1>
            <p class="text-sm text-muted">
              Stage agency budget rows, run ten validation rules, repair failures with AI-assisted guidance, and transform clean rows into CivicFlow requests in one move.
            </p>
          </div>
          <div class="row gap-2">
            <button type="button" class="btn btn-primary" (click)="runSampleImport()">
              <app-icon name="upload" [size]="14"></app-icon> Run sample import
            </button>
          </div>
        </div>

        <!-- Upload area / pipeline stages -->
        <div *ngIf="!summary" class="dropzone" (click)="runSampleImport()">
          <div class="dz-icon"><app-icon name="upload" [size]="24"></app-icon></div>
          <div class="dz-title">Drop a budget CSV here, or click to load the demo batch</div>
          <div class="dz-sub">Demo batch contains 1 valid row, 1 with bad reference codes, 1 over the auto-import threshold</div>
        </div>

        <!-- Pipeline stages -->
        <div *ngIf="summary" class="grid grid-4 gap-4">
          <div class="kpi">
            <span class="kpi-label">Total rows</span>
            <span class="kpi-value">{{ summary.totalRows }}</span>
            <span class="kpi-trend">{{ summary.fileName }}</span>
          </div>
          <div class="kpi kpi-green">
            <span class="kpi-label">Accepted</span>
            <span class="kpi-value">{{ summary.acceptedRows }}</span>
            <span class="kpi-trend">ready for transform</span>
          </div>
          <div class="kpi" style="--bar: var(--red-500);">
            <span class="kpi-label">Rejected</span>
            <span class="kpi-value">{{ summary.rejectedRows }}</span>
            <span class="kpi-trend">{{ explanationBatch ? 'AI-explained' : 'needs explanation' }}</span>
          </div>
          <div class="kpi kpi-amber">
            <span class="kpi-label">Status</span>
            <span class="kpi-value" style="font-size: var(--text-xl); padding-top: 0.5rem;">{{ summary.status }}</span>
            <span class="kpi-trend" *ngIf="explanationBatch">{{ '$' + (explanationBatch.totalEstimatedCostUsd | number:'1.6-6') }} AI cost</span>
          </div>
        </div>

        <div *ngIf="summary" class="row gap-2" style="flex-wrap: wrap;">
          <button type="button" class="btn btn-ai" (click)="explainErrors()" [disabled]="!summary || summary.rejectedRows === 0 || explaining">
            <app-icon name="sparkles" [size]="14"></app-icon>
            <span *ngIf="!explaining">Explain failures with AI</span>
            <span *ngIf="explaining" class="row gap-2"><span class="spinner"></span> Asking model…</span>
          </button>
          <button type="button" class="btn btn-accent" (click)="transformBatch()" [disabled]="!summary || summary.acceptedRows === 0">
            <app-icon name="check" [size]="14"></app-icon> Transform valid rows
          </button>
          <button type="button" class="btn btn-ghost" (click)="reset()">
            <app-icon name="refresh" [size]="14"></app-icon> Reset
          </button>
        </div>
      </div>

      <!-- Row cards -->
      <div *ngIf="summary" class="stack gap-3">
        <div *ngFor="let row of summary.rows" class="card card-pad stack gap-2"
             [style.borderLeft]="rowBorder(row)">
          <div class="row gap-3" style="justify-content: space-between; align-items: baseline; flex-wrap: wrap;">
            <div class="row gap-2" style="align-items: baseline;">
              <span class="text-xs text-muted">Row {{ row.rowNumber }}</span>
              <span class="request-number">{{ row.requestNumber || '—' }}</span>
              <span class="pill" [ngClass]="rowPill(row.status)">{{ row.status }}</span>
            </div>
            <span class="text-xs text-muted" *ngIf="!row.errors.length">No validator errors</span>
          </div>

          <ul *ngIf="row.errors.length" class="stack gap-1" style="padding-left: 1.2rem; margin: 0; color: var(--red-600); font-size: var(--text-xs);">
            <li *ngFor="let e of row.errors">{{ e }}</li>
          </ul>

          <div *ngIf="explanationFor(row.rowNumber) as ex" class="ai-panel">
            <div class="ai-panel-header">
              <div class="title">
                <app-icon name="sparkles" [size]="14"></app-icon>
                AI explanation
              </div>
              <div class="ai-panel-meta">
                <span [ngClass]="ex.servedFromMock ? 'badge badge-mock' : ex.servedFromKillSwitch ? 'badge badge-killswitch' : 'badge badge-live'">
                  {{ ex.servedFromKillSwitch ? 'kill-switch' : ex.servedFromMock ? 'mock' : 'live · ' + ex.providerName }}
                </span>
                <span>{{ ex.confidence }} conf.</span>
                <span>{{ ex.latencyMs }}ms</span>
                <span>{{ '$' + (ex.estimatedCostUsd | number:'1.6-6') }}</span>
              </div>
            </div>
            <div class="ai-panel-body">
              <p class="summary">{{ ex.summary }}</p>
              <div class="field-guidance" *ngIf="ex.fieldGuidance?.length">
                <div *ngFor="let g of ex.fieldGuidance" class="guidance-row">
                  <span class="field-name">{{ g.field }}</span>
                  <div class="stack">
                    <span class="problem">{{ g.problem }}</span>
                    <span class="fix">→ {{ g.fix }}</span>
                  </div>
                </div>
              </div>
              <div class="agency-message" *ngIf="ex.agencyMessage">
                <strong>For the agency:</strong> {{ ex.agencyMessage }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
})
export class ImportRepairCenterComponent {
  summary?: ImportBatchSummary;
  explanationBatch?: ImportErrorExplanationBatch;
  explaining = false;

  constructor(private readonly api: CivicFlowApiService, private readonly ctx: UserContextService) {}

  runSampleImport(): void {
    const userId = this.ctx.currentUserId;
    if (!userId) return;
    this.explanationBatch = undefined;
    const rows: ImportRow[] = [
      { rowNumber: 1, requestNumber: 'REQ-1001', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 1200, title: 'Valid budget correction', effectiveDateText: '2026-07-01' },
      { rowNumber: 2, requestNumber: 'REQ-1002', agencyCode: 'BAD', fundCode: 'NOPE', programCode: 'BUD', fiscalYear: 2026, amount: 500, title: 'Invalid agency/fund', effectiveDateText: '2026-07-01' },
      { rowNumber: 3, requestNumber: 'REQ-1003', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 6000000, title: 'Over threshold', effectiveDateText: '2026-07-01' }
    ];
    this.api.createImportBatch('sample-budget-import.csv', userId, rows).subscribe(s => this.summary = s);
  }

  explainErrors(): void {
    const id = this.ctx.currentUserId;
    if (!this.summary || !id) return;
    this.explaining = true;
    this.api.explainImportErrors(this.summary.id, id).subscribe({
      next: b => { this.explanationBatch = b; this.explaining = false; },
      error: () => this.explaining = false,
    });
  }

  transformBatch(): void {
    const id = this.ctx.currentUserId;
    if (!this.summary || !id) return;
    this.api.transformImportBatch(this.summary.id, id).subscribe(s => this.summary = s);
  }

  reset(): void { this.summary = undefined; this.explanationBatch = undefined; }

  explanationFor(n: number) { return this.explanationBatch?.explanations.find(e => e.rowNumber === n); }

  rowBorder(row: { errors: string[]; status: string }): string {
    if (row.status === 'Transformed') return '4px solid var(--green-500)';
    if (row.errors.length > 0) return '4px solid var(--red-500)';
    if (row.status === 'Valid') return '4px solid var(--green-500)';
    return '4px solid var(--ink-200)';
  }
  rowPill(status: string): string {
    if (status === 'Valid' || status === 'Transformed') return 'pill-green';
    if (status === 'Rejected') return 'pill-amber';
    return '';
  }
}
