# CivicFlow Demo Script

**Goal.** A 5- to 7-minute walkthrough that lands the WOW project for a non-developer interviewer while leaving room for a developer interviewer to ask deeper questions.

**Format.** Screen recording with talking-head off (or picture-in-picture if your camera frame is good). Run at https://waofm-demo.madewellrd.com or locally via `docker compose -f docker-compose.demo.yml up -d --build`.

**Pre-flight (90 seconds, do this before you press record).**

- Browser: 1280x800 window. Zoomed to 100%. No bookmarks bar.
- Demo host: `docker compose -f docker-compose.demo.yml up -d --build` finished, all 3 containers healthy.
- API key: `AI_PROVIDER=Anthropic` and `ANTHROPIC_API_KEY` set on the demo host. Sanity-check: hit `/api/auth/users` with a header for Riley Requester (`10000000-0000-0000-0000-000000000001`); you should see 12 users.
- Reset state: `docker compose -f docker-compose.demo.yml down -v && docker compose -f docker-compose.demo.yml up -d --build` if you want a clean seed.
- Role switcher: confirmed visible top-right.

---

## Scene 1 — Frame the project (0:00 – 0:45)

**Show:** Slide 1 of the deck (CivicFlow title), then slide 2 (the pitch).

**Narration:**
> CivicFlow is a portfolio-grade .NET 8 application I built to demonstrate, in code, exactly what the OFM IT Application Developer job description asks for. ASP.NET Core 8 minimal APIs, EF Core to SQL Server, T-SQL stored procedures, Angular 18, and a Platform layer modeled directly on ServiceNow's Business Rule, Transform Map, and UI Policy concepts. On top of that, two AI features that bring the same governance-first MLOps patterns I built at Providence and the City of Seattle into a .NET app a state agency could actually run.

---

## Scene 2 — Role-based dashboard (0:45 – 1:45)

**Show:** Open the SPA at the demo URL. Role switcher set to **Riley Requester**.

**Click path:**
1. Look at the request list — pre-seeded with 15 requests across every workflow state.
2. Drop the role switcher and pick **Bailey Analyst**.
3. Observe that the actions visible on each card change (the AI triage button stays, but the role-gated buttons differ).
4. Switch to **Avery Approver** — now the Approve action lights up.

**Narration:**
> The demo seeds twelve users across six roles. The role switcher in the corner sets a header on every request — `X-CivicFlow-User`. The backend is a real ASP.NET Core authentication scheme that reads that header, looks the user up in SQL Server, and attaches their role as a claim. From there, eleven named authorization policies gate every endpoint. The whole pattern moves to Entra ID in production by swapping a single scheme registration — the policy table stays the same.

---

## Scene 3 — AI triage router on a real request (1:45 – 3:00)

**Show:** Switch back to **Bailey Analyst**. Pick a Submitted request in the list (e.g., "Transportation fund supplemental request"). Click **AI triage**.

**Narration over the 2-3 second LLM round trip:**
> When I click AI triage, the backend pulls four most-recent requests in the same category as grounding context, builds a prompt that includes the new request's title, justification, and estimated amount, and asks the model for a queue assignment, a complexity score, and whether human review is required. The model has to respond against a JSON schema I registered on startup. If the output doesn't match, the adapter rejects it before the row ever leaves the API boundary.

**Show:** The recommendation panel appears with: queue, complexity, human-review flag, similar past requests, plus a one-line provenance string — provider, confidence, latency in ms, USD cost, mock/kill-switch flags.

**Narration:**
> Notice the metadata line: it shows me which provider answered, how long it took, and the USD cost of the call. That's the exact telemetry I built into PROMETHEUS — and it ships into CivicFlow as a record type that flows through every adapter in the chain.

---

## Scene 4 — Kill-switch as governance (3:00 – 3:45)

**Show:** Bring up a terminal or `docker compose exec api` and demonstrate flipping `Ai__KillSwitchEngaged=true` via env, OR just narrate the path on the slide.

**Narration:**
> Now imagine OFM legal or a budget oversight committee says "shut off the AI features today." Production teams have all seen panicked deploys to do that. In CivicFlow, the kill-switch is one config flag. The KillSwitchAdapter is a decorator around the real provider. When it's engaged, every AI invocation returns a deterministic safe default — for the triage router, that's "Budget Operations, human review required, low confidence" — and writes a kill-switch event to the audit log. No deploy, no code change, observable in audit.

**Show:** Click AI triage again with the switch on. The recommendation comes back with the safe defaults and the kill-switch badge visible.

---

## Scene 5 — Data Integration Repair Center + Error Explainer (3:45 – 5:30)

**Show:** Navigate to the Import Repair Center. Click **Run sample import**.

**Narration:**
> This is the Data Integration Repair Center. It mirrors the kind of nightly agency data load that lands in OFM's stack today — staged rows that need to be validated, fixed, and turned into requests.

**Show:** Three sample rows appear: one valid, one with bad reference codes, one over the auto-import threshold. Errors are listed under the failing rows.

**Narration:**
> Ten validation rules ran server-side. Row two fails on agency code and fund code; row three fails on amount. Those are the raw validator errors. Useful for me, not so useful for the agency analyst who needs to fix the source data.

**Show:** Click **Explain failures with AI**. Wait the 2-3 seconds.

**Narration:**
> The Import Error Explainer takes each failed row plus its validator errors and asks the model to produce, against a JSON schema, an agency-facing message, per-field guidance with concrete fixes, and a confidence score. Same governance pattern — schema enforcement, cost telemetry, kill-switch — different prompt.

**Show:** Per-row explanations render with agency message + field guidance + provenance metadata.

---

## Scene 6 — Business rule firing in the audit log (5:30 – 6:15)

**Show:** Submit a new high-value request via the New Request form, set amount to $2,000,000. After it lands in Submitted, query the audit log table (or expose a UI page if you build one — for the recording, run `SELECT TOP 20 ActorUserId, ActionType, Summary FROM dbo.AuditLogs ORDER BY OccurredAt DESC` in SSMS or a quick `dotnet ef dbcontext optimize` window).

**Narration:**
> When the request transitioned into Submitted, the BusinessRuleEngine ran the After-phase rules for the Request table. The Oversight Threshold rule fired — it's the CivicFlow equivalent of a ServiceNow Business Rule on the Submitted event, with a condition of `amount >= $1,000,000`. The rule wrote its own audit entry. The Legacy Integration Tag rule didn't fire because the category wasn't Legacy.

---

## Scene 7 — Close (6:15 – 6:45)

**Show:** Back to slide 11 ("What I'd build next").

**Narration:**
> CivicFlow is two days of work. It's not production. The next moves are real auth via Entra, OpenTelemetry traces, a proper EF migration regeneration, and an embeddings-backed retrieval layer for the triage router. But I built this to answer the question my resume might raise: can I write the .NET and SQL and Angular and ServiceNow-shape code OFM actually maintains? CivicFlow says yes. I'd love to talk through the production stories behind every pattern in it.

---

## Recording tips

- One take per scene is fine. Cut between scenes; don't try to do it in one shot.
- Don't say "um" while the model is thinking — narrate the pipeline instead, which is exactly what's interesting.
- Keep mouse motion deliberate and slow. Hover before you click so the audience tracks where you are.
- Render at 1080p H.264. Target file size <100MB for emailability.

## Fallback if live demo fails on interview day

1. Bring the recorded video on a thumb drive.
2. Bring the deck as a PDF and pptx — both render correctly on Office and Google Slides.
3. Have the code open in VS Code with `Program.cs`, `AuthRegistration.cs`, `TriageRouterService.cs`, `OversightThresholdBusinessRule.cs`, and `AnthropicAdapter.cs` already pinned in tabs.
