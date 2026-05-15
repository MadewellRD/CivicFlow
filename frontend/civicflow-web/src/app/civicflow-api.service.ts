import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, InjectionToken, Optional } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CivicRequest,
  CreateRequest,
  CurrentUser,
  ImportBatchSummary,
  ImportErrorExplanationBatch,
  ImportRow,
  RosterUser,
  TriageRecommendation
} from './models';

/** Optional override token so we can point the SPA at the prod API host. */
export const CIVICFLOW_API_BASE_URL = new InjectionToken<string>('CIVICFLOW_API_BASE_URL');

@Injectable({ providedIn: 'root' })
export class CivicFlowApiService {
  private readonly baseUrl: string;

  constructor(
    private readonly http: HttpClient,
    @Optional() @Inject(CIVICFLOW_API_BASE_URL) configuredBaseUrl: string | null
  ) {
    this.baseUrl = configuredBaseUrl ?? defaultApiBaseUrl();
  }

  // Identity
  me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/auth/me`);
  }

  listUsers(): Observable<RosterUser[]> {
    return this.http.get<RosterUser[]>(`${this.baseUrl}/auth/users`);
  }

  // Requests
  listRequests(): Observable<CivicRequest[]> {
    return this.http.get<CivicRequest[]>(`${this.baseUrl}/requests`);
  }

  createRequest(request: CreateRequest): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests`, request);
  }

  submitRequest(id: string, actorUserId: string): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests/${id}/submit?actorUserId=${actorUserId}`, {});
  }

  triageRequest(id: string, actorUserId: string): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests/${id}/triage?actorUserId=${actorUserId}`, {});
  }

  approveRequest(id: string, actorUserId: string): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests/${id}/approve?actorUserId=${actorUserId}`, {});
  }

  recommendTriage(id: string): Observable<TriageRecommendation> {
    return this.http.post<TriageRecommendation>(`${this.baseUrl}/requests/${id}/triage-recommendation`, {});
  }

  // Imports
  createImportBatch(fileName: string, uploadedByUserId: string, rows: ImportRow[]): Observable<ImportBatchSummary> {
    return this.http.post<ImportBatchSummary>(`${this.baseUrl}/imports/budget-requests`, { fileName, uploadedByUserId, rows });
  }

  transformImportBatch(batchId: string, actorUserId: string): Observable<ImportBatchSummary> {
    return this.http.post<ImportBatchSummary>(`${this.baseUrl}/imports/${batchId}/transform`, { actorUserId });
  }

  explainImportErrors(batchId: string, actorUserId: string): Observable<ImportErrorExplanationBatch> {
    return this.http.post<ImportErrorExplanationBatch>(`${this.baseUrl}/imports/${batchId}/explain-errors`, { actorUserId });
  }
}

function defaultApiBaseUrl(): string {
  if (typeof location !== 'undefined' && location.hostname !== 'localhost' && location.hostname !== '127.0.0.1') {
    return '/api';
  }

  return 'http://localhost:5000/api';
}
