import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CivicFlowApiService } from './civicflow-api.service';
import { ImportBatchSummary, ImportRow } from './models';

@Component({
  selector: 'app-import-repair-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h1>Data Integration Repair Center</h1>
      <p>Validate agency budget import rows before transforming them into CivicFlow requests.</p>
      <button type="button" (click)="runSampleImport()">Run sample import</button>
    </section>

    <section class="card" *ngIf="summary">
      <h2>{{ summary.fileName }} - {{ summary.status }}</h2>
      <p>Total: {{ summary.totalRows }} | Accepted: {{ summary.acceptedRows }} | Rejected: {{ summary.rejectedRows }}</p>
      <button type="button" (click)="transform()" [disabled]="summary.acceptedRows === 0 || summary.status === 'Transformed'">Transform valid rows</button>
      <article class="card" *ngFor="let row of summary.rows">
        <strong>Row {{ row.rowNumber }} - {{ row.requestNumber }}</strong>
        <span class="status">{{ row.status }}</span>
        <ul *ngIf="row.errors.length">
          <li class="error" *ngFor="let error of row.errors">{{ error }}</li>
        </ul>
      </article>
    </section>
  `
})
export class ImportRepairCenterComponent {
  readonly demoRequesterId = '10000000-0000-0000-0000-000000000001';
  summary?: ImportBatchSummary;

  constructor(private readonly api: CivicFlowApiService) {}

  transform(): void {
    if (!this.summary) return;
    this.api.transformImportBatch(this.summary.id, this.demoRequesterId).subscribe(summary => this.summary = summary);
  }

  runSampleImport(): void {
    const rows: ImportRow[] = [
      { rowNumber: 1, requestNumber: 'REQ-1001', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 1200, title: 'Valid budget correction', effectiveDateText: '2026-07-01' },
      { rowNumber: 2, requestNumber: 'REQ-1002', agencyCode: 'BAD', fundCode: 'NOPE', programCode: 'BUD', fiscalYear: 2026, amount: 500, title: 'Invalid agency/fund', effectiveDateText: '2026-07-01' },
      { rowNumber: 3, requestNumber: 'REQ-1003', agencyCode: 'OFM', fundCode: 'GF-S', programCode: 'BUD', fiscalYear: 2026, amount: 6000000, title: 'Over threshold', effectiveDateText: '2026-07-01' }
    ];
    this.api.createImportBatch('sample-budget-import.csv', this.demoRequesterId, rows).subscribe(summary => this.summary = summary);
  }
}
