import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CivicRequest, CreateRequest, ImportBatchSummary, ImportRow } from './models';

@Injectable({ providedIn: 'root' })
export class CivicFlowApiService {
  private readonly baseUrl = 'http://localhost:5000/api';

  constructor(private readonly http: HttpClient) {}

  listRequests(): Observable<CivicRequest[]> {
    return this.http.get<CivicRequest[]>(`${this.baseUrl}/requests`);
  }

  createRequest(request: CreateRequest): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests`, request);
  }

  submitRequest(id: string, actorUserId: string): Observable<CivicRequest> {
    return this.http.post<CivicRequest>(`${this.baseUrl}/requests/${id}/submit?actorUserId=${actorUserId}`, {});
  }

  createImportBatch(fileName: string, uploadedByUserId: string, rows: ImportRow[]): Observable<ImportBatchSummary> {
    return this.http.post<ImportBatchSummary>(`${this.baseUrl}/imports/budget-requests`, { fileName, uploadedByUserId, rows });
  }

  transformImportBatch(batchId: string, actorUserId: string): Observable<ImportBatchSummary> {
    return this.http.post<ImportBatchSummary>(`${this.baseUrl}/imports/${batchId}/transform`, { actorUserId });
  }
}
