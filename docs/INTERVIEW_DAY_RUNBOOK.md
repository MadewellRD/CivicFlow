# Interview Day Runbook — 2026-05-15

A compact "what to do, when" list for the day of the OFM IT Application Developer interview.

## T-12 hours (night before)

- [ ] Open `docs/CivicFlow.pptx` once and click through end to end. Confirm no rendering issues on your machine.
- [ ] Open `docs/DEMO_SCRIPT.md` and read it out loud once, timing each scene. Target 5–7 minutes.
- [ ] Open `docs/STAR_TALKING_POINTS.md` on a second monitor; skim the 10 stories.
- [ ] Sleep.

## T-3 hours

- [ ] On the demo host, `docker compose -f docker-compose.demo.yml pull` to refresh the SQL Server image.
- [ ] `docker compose -f docker-compose.demo.yml down -v && docker compose -f docker-compose.demo.yml up -d --build` for a clean seed.
- [ ] `curl -s -H 'X-CivicFlow-User: 10000000-0000-0000-0000-000000000001' https://waofm-demo.madewellrd.com/api/requests | head` — confirm seeded requests are listed.

## T-90 minutes

- [ ] Run a smoke AI call. Sign in to the SPA as Bailey Analyst. Click AI triage on any submitted request. Confirm a real response renders with cost > $0.
- [ ] Flip kill-switch: SSH into the host, set `Ai__KillSwitchEngaged=true` in the env, restart the API container, retry the triage button, confirm safe-default payload and kill-switch badge.
- [ ] Flip it back off. Confirm normal behavior returns.
- [ ] Hard-refresh the SPA in your interview-day browser (Ctrl+Shift+R). No stale assets.

## T-30 minutes

- [ ] Close every app you don't need. Browser, IDE, deck, terminal. Nothing else.
- [ ] Browser layout: Tab 1 = deck PDF, Tab 2 = SPA at waofm-demo, Tab 3 = `docs/STAR_TALKING_POINTS.md`, Tab 4 = `docs/DEMO_SCRIPT.md`. IDE: `Program.cs`, `AuthRegistration.cs`, `TriageRouterService.cs`, `OversightThresholdBusinessRule.cs`, `AnthropicAdapter.cs` pinned.
- [ ] Water on the desk. Pen. Notepad. Quiet room. Camera framed.

## T-5 minutes

- [ ] Join the meeting link. Camera and mic check. Share-screen rehearsal — confirm the SPA renders correctly when shared at the resolution the panel will see.

## During the interview

- [ ] Lead with the self-positioning anchor from STAR_TALKING_POINTS.md.
- [ ] Offer the live demo within the first 10 minutes. Don't wait for them to ask.
- [ ] When they ask a question, name the prior-work story first, then bridge to CivicFlow.
- [ ] If a demo step fails, switch to the recorded video. Don't fight it.
- [ ] Close with "I'd love to walk through any production story behind any pattern you saw."

## Within 24 hours after

- [ ] Thank-you email to every panelist. Two paragraphs max. Reference one specific moment from the conversation.
- [ ] Update `docs/INCIDENT_CASE_STUDY.md` if any technical question surfaced a real gap in the project; honest postmortem energy.
