# Incident Case Study: Budget Import Failure for Legacy Fund Codes

## Symptom

Several rows in a budget import batch were rejected after an agency uploaded a CSV containing legacy fund code values.

## Impact

The request intake workflow remained available, but the affected agency could not transform imported rows into CivicFlow requests until the data was corrected.

## Detection

The Import Repair Center showed rejected rows with `FundCode` validation errors. Support staff confirmed the same pattern through `dbo.GetImportBatchSummary`.

## Root cause

The source file used a retired fund code not present in active CivicFlow reference data.

## Corrective action

The agency corrected the CSV using the active fund code. The batch was revalidated and valid rows were transformed.

## Preventive action

Add a downloadable active reference-data template and a clearer pre-upload validation message listing accepted fund codes.

## Regression test

Add an import validation test proving unknown fund codes are rejected with a `FundCode` field error and that valid `GF-S` rows are accepted.
