import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { CIVICFLOW_API_BASE_URL } from './civicflow-api.service';
import { Inject, Optional } from '@angular/core';
import { IconComponent } from './icon.component';

interface BusinessRuleInfo { name: string; table: string; phase: string; order: number; }
interface FieldMap { sourceField: string; targetField: string; transformScript: string | null; }
interface TransformMapInfo { name: string; sourceTable: string; targetTable: string; fieldMaps: FieldMap[]; }
interface UiPolicyInfo { formName: string; field: string; behavior: string; whenExpression: string; }

@Component({
  selector: 'app-platform-inspector',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <section class="stack gap-6">
      <div class="card card-pad stack gap-2">
        <span class="pill pill-amber">ServiceNow-shape platform layer</span>
        <h1>Platform inspector</h1>
        <p class="text-sm text-muted" style="max-width: 760px;">
          The Application layer mirrors three ServiceNow concepts by name and shape: Business Rules with Before/After/Async phases, Transform Maps with declarative field mappings, and UI Policies with conditional form behavior. Every endpoint here returns the same data an OFM ServiceNow admin would see in Studio.
        </p>
      </div>

      <!-- Business rules -->
      <div class="card">
        <div class="card-header">
          <div class="row gap-2"><app-icon name="bolt" [size]="16"></app-icon><h2>Business rules</h2></div>
          <span class="pill pill-blue">{{ rules.length }} active</span>
        </div>
        <table class="table">
          <thead><tr><th>Name</th><th>Table</th><th>Phase</th><th>Order</th></tr></thead>
          <tbody>
            <tr *ngFor="let r of rules">
              <td class="fw-semibold text-navy">{{ r.name }}</td>
              <td><span class="pill">{{ r.table }}</span></td>
              <td><span class="pill" [ngClass]="phaseClass(r.phase)">{{ r.phase }}</span></td>
              <td class="text-mono text-xs">{{ r.order }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Transform maps -->
      <div class="card">
        <div class="card-header">
          <div class="row gap-2"><app-icon name="imports" [size]="16"></app-icon><h2>Transform maps</h2></div>
          <span class="pill pill-blue">{{ maps.length }} active</span>
        </div>
        <div class="card-pad stack gap-5">
          <div *ngFor="let m of maps" class="stack gap-3">
            <div class="row gap-3" style="justify-content: space-between; align-items: baseline;">
              <h3 style="font-size: var(--text-base);">{{ m.name }}</h3>
              <span class="text-xs text-muted">{{ m.sourceTable }} → {{ m.targetTable }}</span>
            </div>
            <table class="table">
              <thead><tr><th>Source field</th><th>Target field</th><th>Transform</th></tr></thead>
              <tbody>
                <tr *ngFor="let f of m.fieldMaps">
                  <td class="text-mono text-xs">{{ f.sourceField }}</td>
                  <td class="text-mono text-xs">{{ f.targetField }}</td>
                  <td class="text-xs text-muted">{{ f.transformScript || 'passthrough' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- UI policies -->
      <div class="card">
        <div class="card-header">
          <div class="row gap-2"><app-icon name="shield" [size]="16"></app-icon><h2>UI policies</h2></div>
          <span class="pill pill-blue">{{ policies.length }} active</span>
        </div>
        <table class="table">
          <thead><tr><th>Form</th><th>Field</th><th>Behavior</th><th>When</th></tr></thead>
          <tbody>
            <tr *ngFor="let p of policies">
              <td class="text-mono text-xs">{{ p.formName }}</td>
              <td class="text-mono text-xs">{{ p.field }}</td>
              <td><span class="pill" [ngClass]="behaviorClass(p.behavior)">{{ p.behavior }}</span></td>
              <td class="text-xs text-muted">{{ p.whenExpression }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  `,
})
export class PlatformInspectorComponent implements OnInit {
  rules: BusinessRuleInfo[] = [];
  maps: TransformMapInfo[] = [];
  policies: UiPolicyInfo[] = [];
  private readonly base: string;
  constructor(private readonly http: HttpClient, @Optional() @Inject(CIVICFLOW_API_BASE_URL) base: string | null) {
    this.base = base ?? 'http://localhost:5000/api';
  }
  ngOnInit(): void {
    this.http.get<BusinessRuleInfo[]>(`${this.base}/platform/business-rules`).subscribe(d => this.rules = d);
    this.http.get<TransformMapInfo[]>(`${this.base}/platform/transform-maps`).subscribe(d => this.maps = d);
    // pull UI policies for the two known forms
    this.http.get<UiPolicyInfo[]>(`${this.base}/platform/ui-policies/new-request`).subscribe(d => this.policies = [...this.policies, ...d]);
    this.http.get<UiPolicyInfo[]>(`${this.base}/platform/ui-policies/import-row-fix`).subscribe(d => this.policies = [...this.policies, ...d]);
  }
  phaseClass(p: string): string { return p === 'Before' ? 'pill-blue' : p === 'After' ? 'pill-green' : 'pill-amber'; }
  behaviorClass(b: string): string {
    if (b === 'mandatory') return 'pill-amber';
    if (b === 'readonly') return 'pill-blue';
    if (b === 'warn') return 'pill-amber';
    return 'pill-green';
  }
}
