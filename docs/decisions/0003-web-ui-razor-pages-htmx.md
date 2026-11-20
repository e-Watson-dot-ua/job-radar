# 0003 — Web UI: Razor Pages with htmx instead of Angular

- Status: accepted
- Date: 2026-11-20

## Context

The original phase-1 brief specified an Angular single-page app. The UI needs for phase 1 are a
filtered postings list, a detail view, and a small pipeline status widget: forms, links, and
partial page updates. The project's priority is a working tool with AI features (LLM extraction
now; matching and RAG chat later), all of which live in the backend.

## Options

1. **Angular** — rich client-side interactivity and a common skill in .NET job postings, but adds
   a second language and toolchain (TypeScript, Node, npm, Angular CLI), a separate build, a typed
   API client, and extra Aspire wiring.
2. **Razor Pages + htmx** — server-rendered HTML in the same .NET solution; htmx adds partial
   updates and URL-synced filters with HTML attributes. One language, one toolchain.

## Decision

Use ASP.NET Core Razor Pages with htmx in a `JobRadar.Web` project. Pages call Application use
cases directly. htmx is vendored as a pinned file in the repo (no CDN at runtime, no npm).
The JSON API (`JobRadar.Api`) stays for stats, tests, and later phases.

## Consequences

- No Node prerequisite; `dotnet run --project src/JobRadar.AppHost` starts everything.
- Fewer moving parts; UI behaviour can be covered by .NET integration tests.
- Streaming (later RAG chat) is possible with the htmx SSE extension.
- If the UI later needs heavy client-side interactivity, this decision should be revisited.
