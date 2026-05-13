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
