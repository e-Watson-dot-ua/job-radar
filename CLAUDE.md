# Job Radar — Project Rules

Job Radar collects public job postings, extracts structured data from them with an LLM,
and helps a single user analyse the market and match postings against their own profile.

This file holds stable project rules. Task-specific scope lives in `docs/briefs/`.

## Language

- All code, comments, commit messages, docs, log messages, and UI text are in English.
- Commit messages follow Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).

## Stack

- Backend: .NET 10, ASP.NET Core minimal APIs, EF Core with Npgsql, PostgreSQL.
- Local orchestration: Aspire (AppHost + ServiceDefaults).
- LLM access: `Microsoft.Extensions.AI` abstractions (`IChatClient`) only. No provider SDK types
  outside the infrastructure layer. Local provider: Ollama.
- Frontend: ASP.NET Core Razor Pages with htmx (server-rendered, no Node toolchain).
  htmx is vendored as a pinned file in the repo, not loaded from a CDN.
- Tests: xUnit, Testcontainers for PostgreSQL integration tests.

## Architecture rules

- Layers: `Domain` (no dependencies) → `Application` → `Infrastructure` → hosts (`Api`, `Web`, workers).
- Every external dependency (job source, LLM, clock) sits behind an interface so it can be faked in tests.
- Background work is idempotent: re-running any step must not create duplicates or corrupt data.
- Never invent data. If a field is not stated in a posting, store `null`, not a guess.
- Configuration through `appsettings*.json` and environment variables. No secrets in the repo.
- Personal data (CV, profile, private notes) lives only under `data/private/`, which is git-ignored.
  Never commit, log, or print its contents.

## Working agreement

- Before writing code for a non-trivial task, outline the plan and the files you will touch.
- Work in small, reviewable steps; each step builds and passes tests before moving on.
- Do not add a NuGet or npm package without stating why it is needed.
- When requirements are ambiguous, ask instead of assuming.
- Stay within the scope of the current brief. Note out-of-scope ideas in `docs/backlog.md` instead of implementing them.
- Keep `README.md` runnable: a fresh clone plus the documented steps must start the system.

## Commands

- Run everything: `dotnet run --project src/JobRadar.AppHost`
- Tests: `dotnet test`

## Data source etiquette

- Use only sources with an official public API or feed.
- Respect each source's terms: rate-limit requests, identify the client with a descriptive `User-Agent`,
  and show attribution with a link to the source wherever its postings are displayed.
