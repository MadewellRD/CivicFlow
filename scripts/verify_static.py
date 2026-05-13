#!/usr/bin/env python3
from pathlib import Path
import json
import sys
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
failures: list[str] = []

required_files = [
    'CivicFlow.sln',
    'docker-compose.yml',
    '.github/workflows/ci.yml',
    'src/CivicFlow.Domain/CivicFlow.Domain.csproj',
    'src/CivicFlow.Application/CivicFlow.Application.csproj',
    'src/CivicFlow.Infrastructure/CivicFlow.Infrastructure.csproj',
    'src/CivicFlow.Api/CivicFlow.Api.csproj',
    'tests/CivicFlow.Tests/CivicFlow.Tests.csproj',
    'src/CivicFlow.Domain/Entities/Request.cs',
    'src/CivicFlow.Domain/Entities/RequestWorkflow.cs',
    'src/CivicFlow.Application/Services/RequestWorkflowService.cs',
    'src/CivicFlow.Application/Services/ImportValidationService.cs',
    'src/CivicFlow.Infrastructure/Persistence/CivicFlowDbContext.cs',
    'src/CivicFlow.Api/Program.cs',
    'database/stored-procedures/001_validate_import_batch.sql',
    'frontend/civicflow-web/package.json',
    'docs/ARCHITECTURE.md',
    'docs/API_CONTRACT.md',
    'docs/DATABASE_SCHEMA.md',
    'docs/TEST_PLAN.md',
    'docs/RUNBOOK.md',
    'docs/INCIDENT_CASE_STUDY.md',
]

for rel in required_files:
    if not (root / rel).exists():
        failures.append(f'Missing required file: {rel}')

for csproj in root.glob('**/*.csproj'):
    try:
        ET.parse(csproj)
    except ET.ParseError as exc:
        failures.append(f'Invalid XML in {csproj.relative_to(root)}: {exc}')

for json_file in [root / 'global.json', root / 'frontend/civicflow-web/package.json', root / 'frontend/civicflow-web/angular.json']:
    try:
        json.loads(json_file.read_text())
    except Exception as exc:
        failures.append(f'Invalid JSON in {json_file.relative_to(root)}: {exc}')

program = (root / 'src/CivicFlow.Api/Program.cs').read_text()
for endpoint in ['/api', '/requests', '/imports/budget-requests', '/health', '/integrations/legacy-budget']:
    if endpoint not in program:
        failures.append(f'Expected endpoint marker not found in Program.cs: {endpoint}')

workflow = (root / 'src/CivicFlow.Domain/Entities/RequestWorkflow.cs').read_text()
for status in ['Draft', 'Submitted', 'Triage', 'AnalystReview', 'TechnicalReview', 'Approved', 'Implemented', 'Closed']:
    if status not in workflow:
        failures.append(f'Workflow status missing: {status}')

import_service = (root / 'src/CivicFlow.Application/Services/ImportValidationService.cs').read_text()
for rule in ['AgencyCode', 'FundCode', 'FiscalYear', 'Amount', 'EffectiveDate']:
    if rule not in import_service:
        failures.append(f'Import validation rule missing: {rule}')

tests = '\n'.join(path.read_text() for path in (root / 'tests').glob('**/*.cs'))
for assertion in ['InvalidTransitionThrowsDomainException', 'InvalidImportRowIsRejectedWithFieldErrors', 'ValidImportRowIsAccepted']:
    if assertion not in tests:
        failures.append(f'Expected test missing: {assertion}')

if failures:
    print('STATIC VERIFICATION FAILED')
    for failure in failures:
        print(f'- {failure}')
    sys.exit(1)

print('STATIC VERIFICATION PASSED')
print(f'Repository: {root}')
print(f'C# files: {len(list(root.glob("**/*.cs")))}')
print(f'Documentation files: {len(list((root / "docs").glob("*.md")))}')
