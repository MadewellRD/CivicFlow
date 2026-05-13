# Security Threat Model

## Trust boundaries

| Boundary | Risk | Control |
|---|---|---|
| Browser to API | forged request or unauthorized workflow action | server-side authorization must be enforced before production use |
| CSV upload to import service | malicious or malformed input | file type/size limits, row validation, no dynamic SQL |
| API to SQL Server | data tampering or injection | EF parameterization, stored procedures without dynamic SQL |
| Operator to audit data | audit deletion or manipulation | append-only convention, restricted write access |
| API to legacy integration mock | timeout or bad external response | correlation IDs, timeouts, retry policy in future issue |

## Key risks

| Risk | Impact | Likelihood | Mitigation |
|---|---|---:|---|
| Missing endpoint authorization | high | medium | add ASP.NET Identity/demo auth handler before public demo |
| Overly permissive CORS | medium | low | restrict to Angular dev origin only in appsettings |
| CSV formula injection | medium | medium | sanitize exported CSV values in future export feature |
| PII leakage in logs | high | low | use structured logs and avoid full request bodies |
| Direct SQL status edits | high | medium | runbook forbids direct status mutation except non-prod recovery |

## Security follow-up issues

1. Add ASP.NET Identity or demo JWT auth.
2. Add role policies for Requester, Analyst, Developer, Approver, Admin, and Auditor.
3. Add upload size/type validation for CSV endpoints.
4. Add problem-details error responses that avoid stack-trace leakage.
5. Add audit log immutability tests.
