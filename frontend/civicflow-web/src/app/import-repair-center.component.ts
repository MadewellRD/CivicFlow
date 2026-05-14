import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { ImportBatchSummary, ImportErrorExplanationBatch, ImportRow } from './models';
import { UserContextService } from './user-context.service';

@Component({
  selector: 'app-import-repair-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h1>Data Integration Repair Center</h1>
      <p>Validate agency budget import rows before transforming them into CivicFlow requests.</p>
      <div class="actions">
        <button type="button" (click)="runSampleImport()">Run sample import</button>
        <button
          type="button"
          (click)="explainErrors()"
          [disabled]="!summary || summary.rejectedRows === 0 || explainingInProgress">
          {{ explainingInProgress ? 'Asking AI…' : 'Explain failures with AI' }}
        </button>
        <button
          type="button"
          (click)="transformBatch()"
          [disabled]="!summary || summary.acceptedRows === 0">
          Transform valid rows
        </button>
      </div>
    </section>

    <section class="card" *ngIf="summary">
      <h2>{{ summary.fileName }} &mdash; {{ summary.status }}</h2>
      <p>Total: {{ summary.totalRows }} | Accepted: {{ summary.acceptedRows }} | Rejected: {{ summary.rejectedRows }}</p>
      <article class="card" *ngFor="let row of summary.rows">
        <strong>Row {{ row.rowNumber }} &mdash; {{ row.requestNumber }}</strong>
        <span class="status">{{ row.status }}</span>
        <ul *ngIf="row.errors.length">
          <li class="error" *ngFor="let error of row.errors">{{ error }}</li>
        </ul>
        <div *ngIf="explanationFor(row.rowNumber) as ex" class="ai-explanation">
          <div class="ai-meta">
            <strong>AI explanation</strong>
            <span>{{ ex.providerName }} &middot; {{ ex.confidence }} confidence &middot; {{ ex.latencyMs }}ms &middot; \${{ ex.estimatedCostUsd | number:'1.6-6' }}
              <span *ngIf="ex.servedFromMock"> &middot; mock</span>
              <span *ngIf="ex.servedFromKillSwitch"> &middot; kill-switch</span>
            </span>
          </div>
          <p>{{ ex.summary }}</p>
          <ul>
            <li *ngFor="let g of ex.fieldGuidance"><strong>{{ g.field }}:</strong> {{ g.problem }} <em>&rarr; {{ g.fix }}</em></li>
          </ul>
          <p class="agency-message"><em>Agency-facing message:</em> {{ ex.agencyMessage }}</p>
        </div>
      </article>
    </section>

    <section class="card" *ngIf="explanationBatch">
      <p>
        Explained {{ explanationBatch.rowsExplained }} of {{ summary?.rejectedRows }} failed rows.
        Estimated total cost: \${{ explanationBatch.totalEstimatedCostUsd | number:'1.6-6' }} USD.
      </p>
    </section>
  `,
  styles: [`
    .ai-explanation { margin-top: 0.75rem; padding: 0.5rem 0.75rem; border-left: 3px solid #3b82f6; background: #f4f9ff; }
    .ai-meta { display: flex; justify-content: space-between; font-size: 0.85rem; color: #1e3a8a; margin-bottom: 0.25rem; }
    .agency-message { font-size: 0.9rem; color: #1f2937; }
    .actions { display: flex; gap: 0.5rem; }
  `]
})
export class ImportRepairCenterComponent {
  summary?: ImportBatchSummary;
  explanationBatch?: ImportErrorExplanationBatch;
  explainingInProgress = false;

  constructor(
    private readonly api: CivicFlowApiService,
    private readonly userContext: UserContextService
  ) {}

  runSampleImport(): void {
    const userId = this.userContext.currentUserId;
    if (!userId) { return; }
    this.explanationBatch = undefined;
    const rows: ImportRow[] = [
      { rowNumber: 1, requestNumber: 'REQ-1001', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 1200, title: 'Valid budget correction', effectiveDateText: '2026-07-01' },
      { rowNumber: 2, requestNumber: 'REQ-1002', agencyCode: 'BAD', fundCode: 'NOPE', programCode: 'BUD', fiscalYear: 2026, amount: 500, title: 'Invalid agency/fund', effectiveDateText: '2026-07-01' },
      { rowNumber: 3, requestNumber: 'REQ-1003', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 6000000, title: 'Over threshold', effectiveDateText: '2026-07-01' }
    ];
    this.api.createImportBatch('sample-budget-import.csv', userId, rows).subscribe(summary => this.summary = summary);
  }

  explainErrors(): void {
    const userId = this.userContext.currentUserId;
    if (!this.summary || !userId) { return; }
    this.explainingInProgress = true;
    this.api.explainImportErrors(this.summary.id, userId).subscribe({
      next: batch => {
        this.explanationBatch = batch;
        this.explainingInProgress = false;
      },
      error: () => { this.explainingInProgress = false; }
    });
  }

  transformBatch(): void {
    const userId = this.userContext.currentUserId;
    if (!this.summary || !userId) { return; }
    this.api.transformImportBatch(this.summary.id, userId).subscribe(summary => this.summary = summary);
  }

  explanationFor(rowNumber: number) {
    return this.explanationBatch?.explanations.find(e => e.rowNumber === rowNumber);
  }
}
