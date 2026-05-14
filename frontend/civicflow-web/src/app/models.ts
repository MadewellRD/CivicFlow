export interface CivicRequest {
  id: string;
  requestNumber: string;
  title: string;
  category: string;
  status: string;
  agencyId: string;
  requesterId: string;
  estimatedAmount: number;
  businessJustification: string;
  submittedAt?: string;
}

export interface CreateRequest {
  title: string;
  category: number;
  agencyId: string;
  requesterId: string;
  fundId?: string | null;
  budgetProgramId?: string | null;
  estimatedAmount: number;
  businessJustification: string;
}

export interface ImportRow {
  rowNumber: number;
  requestNumber: string;
  agencyCode: string;
  fundCode: string;
  programCode: string;
  fiscalYear: number;
  amount: number;
  title: string;
  effectiveDateText: string;
}

export interface ImportBatchSummary {
  id: string;
  fileName: string;
  status: string;
  totalRows: number;
  acceptedRows: number;
  rejectedRows: number;
  rows: Array<{ rowNumber: number; requestNumber: string; status: string; errors: string[] }>;
}

export interface RosterUser {
  id: string;
  displayName: string;
  email: string;
  primaryRole: string;
}

export interface CurrentUser {
  userId: string;
  displayName: string;
  role: string;
}

export interface FieldGuidance {
  field: string;
  problem: string;
  fix: string;
}

export interface ImportErrorExplanation {
  rowNumber: number;
  summary: string;
  fieldGuidance: FieldGuidance[];
  agencyMessage: string;
  confidence: string;
  providerName: string;
  servedFromMock: boolean;
  servedFromKillSwitch: boolean;
  inputTokens: number;
  outputTokens: number;
  estimatedCostUsd: number;
  latencyMs: number;
}

export interface ImportErrorExplanationBatch {
  batchId: string;
  fileName: string;
  explanations: ImportErrorExplanation[];
  rowsExplained: number;
  rowsSkipped: number;
  totalEstimatedCostUsd: number;
}

export interface SimilarPastRequest {
  requestNumber: string;
  title: string;
  similarityScore: number;
}

export interface TriageRecommendation {
  requestId: string;
  recommendedQueue: string;
  complexity: string;
  humanReviewRequired: boolean;
  rationale: string;
  similarPastRequests: SimilarPastRequest[];
  confidence: string;
  providerName: string;
  servedFromMock: boolean;
  servedFromKillSwitch: boolean;
  inputTokens: number;
  outputTokens: number;
  estimatedCostUsd: number;
  latencyMs: number;
}
