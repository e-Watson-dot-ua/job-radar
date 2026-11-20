# Brief — Phase 1: Ingestion → Extraction → List

Place this file at `docs/briefs/phase-1.md`. Read `CLAUDE.md` first; its rules apply throughout.

## Goal

A working end-to-end slice: postings are fetched on a schedule from one public source,
an LLM extracts structured fields from each posting, and a web page lists postings with filters.

The slice must be useful on its own and designed so that later phases (messaging, matching,
RAG, evals, Kubernetes) can be added without rewriting it.

## Out of scope for this phase

Message broker, CV matching, embeddings, RAG chat, evals, authentication, Kubernetes, IaC.
If something here seems to need them, write it down in `docs/backlog.md` and continue.

## 1. Solution structure

```
job-radar/
  src/
    JobRadar.AppHost/          Aspire orchestration
    JobRadar.ServiceDefaults/  OpenTelemetry, health checks, resilience defaults
    JobRadar.Domain/           entities, value objects, enums
    JobRadar.Application/      use cases, interfaces (IJobSource, IPostingExtractor, IClock)
    JobRadar.Infrastructure/   EF Core, source clients, LLM extractor, HTML-to-text
    JobRadar.Api/              minimal API
    JobRadar.Web/              Razor Pages + htmx UI
    JobRadar.Ingestion/        worker: fetch and upsert postings
    JobRadar.Extraction/       worker: extract structured fields
  tests/
    JobRadar.UnitTests/
    JobRadar.IntegrationTests/ Testcontainers PostgreSQL
  prompts/
    extraction.v1.md
  data/private/                git-ignored
  docs/
    briefs/phase-1.md
    backlog.md
    decisions/                 short ADRs, one file per decision
```

## 2. Aspire AppHost

- PostgreSQL container with a persistent volume and a `jobradar` database.
- `Api`, `Web`, `Ingestion`, `Extraction` projects referencing the database.
- Ollama: by default the workers call an Ollama instance already running on the host
  (configured endpoint), because host GPU access is simpler. Make it switchable via configuration
  to an Ollama container managed by Aspire. Record the choice in `docs/decisions/0001-ollama-hosting.md`.
- `Web` project registered so that `dotnet run --project src/JobRadar.AppHost` starts everything.

## 3. Data model (PostgreSQL via EF Core migrations)

**`sources`**: `id`, `name`, `base_url`, `attribution_url`.

**`postings`** (raw, as fetched)
- `id` (uuid), `source_id`, `external_id`, `url`
- `title`, `company`, `location_text`, `remote_flag` (as given by the source, nullable)
- `tags` (text[], as given by the source)
- `description_html`, `description_text`
- `raw_payload` (jsonb, the original item)
- `content_hash` (hash of title + description_text)
- `published_at` (nullable), `first_seen_at`, `last_seen_at`
- `extraction_status` (`pending`, `processing`, `succeeded`, `failed`)
- `extraction_attempts`, `last_extraction_error`, `next_attempt_at` (nullable)
- Unique index on (`source_id`, `external_id`).

**`posting_extractions`** (one current row per posting; keep history by `prompt_version`)
- `posting_id`, `prompt_version`, `model_name`, `extracted_at`
- `seniority` (`intern`, `junior`, `middle`, `senior`, `lead`, `principal`, nullable)
- `primary_languages` (text[]), `frameworks` (text[]), `cloud_platforms` (text[]), `other_skills` (text[])
- `salary_min`, `salary_max`, `salary_currency`, `salary_period` (`year`, `month`, `hour`), all nullable
- `remote_policy` (`onsite`, `hybrid`, `remote`, nullable)
- `country`, `city` (nullable)
- `required_spoken_languages` (jsonb: list of `{ language, level }`, level nullable)
- `visa_sponsorship` (bool, nullable), `citizenship_or_clearance_required` (bool, nullable)
- `raw_model_output` (jsonb)
- Primary key (`posting_id`, `prompt_version`).

Normalize skill names to a canonical form (for example `C#`, `.NET`, `Kubernetes`) through
a small mapping table or dictionary in `Application`, unit-tested.

## 4. Ingestion worker

- First source: **Arbeitnow public job board API** (`https://www.arbeitnow.com/api/job-board-api`, paginated
  with `?page=`, no key). Before implementing, read its current documentation and terms and summarize them in
  `docs/decisions/0002-first-source.md`. Its terms ask for a link back; the UI must show it.
- Implement behind `IJobSource` so more sources can be added later.
- Runs on a configurable interval (default every 6 hours) using `PeriodicTimer`; also runnable once on startup.
- Politeness: configurable delay between page requests (default 1 s), configurable max pages per run,
  descriptive `User-Agent`, standard resilience (retry with backoff on transient errors, timeout).
- Upsert by (`source_id`, `external_id`):
  - new posting → insert with `extraction_status = pending`;
  - existing, `content_hash` unchanged → update `last_seen_at` only;
  - existing, `content_hash` changed → update fields and set `extraction_status = pending`.
- Convert `description_html` to clean text (AngleSharp) preserving paragraph and list breaks.
- Log a run summary: pages fetched, new, updated, unchanged, failed.

## 5. Extraction worker

- Polls `postings` for `pending` rows (and `failed` rows whose `next_attempt_at` has passed), in small batches.
- Claims rows safely so two worker instances never process the same posting
  (`SELECT ... FOR UPDATE SKIP LOCKED` or an equivalent update-with-condition).
- For each posting, calls `IPostingExtractor`:
  - Implementation uses `IChatClient` against Ollama with a **JSON schema structured output**.
  - Prompt text is loaded from `prompts/extraction.v1.md`; `prompt_version` = `"extraction.v1"`.
  - The prompt instructs the model to return only what is stated in the posting and `null` otherwise.
  - Model name and endpoint come from configuration. Default to a small instruct model;
    the developer sets the actual model in `appsettings.Development.json`.
- Validate the response: deserialise into a DTO, then apply domain validation
  (enums known, `salary_min <= salary_max`, currency is ISO 4217, etc.).
- On success: write `posting_extractions`, set status `succeeded`.
- On failure: increment `extraction_attempts`, store the error, set `next_attempt_at` with exponential backoff;
  after a configurable maximum (default 3) set status `failed` and stop retrying.
- Record per-call duration and token counts (if the provider reports them) as OpenTelemetry metrics.
- Use a limited parallelism setting (default 1), configurable.

## 6. API

- `GET /api/postings` with query parameters: `q` (title/company text), `country`, `remotePolicy`,
  `seniority`, `skill` (matches any skill array), `hasSalary`, `page`, `pageSize`. Returns paged results
  joined with the latest extraction.
- `GET /api/postings/{id}`: full detail, including extracted fields and source attribution.
- `GET /api/stats/skills?country=&top=20`: most frequent canonical skills among extracted postings.
- `GET /api/stats/pipeline`: counts by `extraction_status`.
- Health checks from ServiceDefaults. ProblemDetails for errors.

## 7. Web UI (Razor Pages + htmx)

- Postings list: table or cards with title, company, location, remote policy, seniority, salary range, top skills.
- Filter panel with the same parameters as `GET /api/postings`; filters reflected in the URL.
- Detail view: extracted fields, the cleaned description, a link to the original posting,
  and "Source: Arbeitnow" with a link.
- Small pipeline status widget (pending / succeeded / failed counts).
- Pages call Application use cases directly (no HTTP hop to the API). htmx handles partial updates:
  filtering results without a full page reload, and refreshing the pipeline widget periodically.
- htmx is vendored as a pinned file under `wwwroot/lib/`; no Node toolchain.

## 8. Tests

- Unit: skill normalisation, HTML-to-text, extraction DTO validation, retry/backoff calculation, content hashing.
- Integration (Testcontainers PostgreSQL): upsert logic (new / unchanged / changed), row claiming
  under two concurrent workers, API filtering.
- The LLM is always faked in tests. A recorded sample response file is enough.
- No test calls a real external API.

## 9. Documentation

- `README.md`: what the project is, architecture diagram (Mermaid), prerequisites (Docker, .NET SDK,
  Ollama), how to run, how to configure the model, source attribution.
- ADRs in `docs/decisions/` for: Ollama hosting, first source, row-claiming strategy, skill normalisation.

## Definition of done

1. A fresh clone plus the README steps starts the whole system with one command.
2. After one ingestion run, postings appear in the database without duplicates on a second run.
3. The extraction worker processes pending postings; failures are retried with backoff and capped.
4. The web UI lists postings, filters work, detail view shows extracted fields and attribution.
5. `/api/stats/skills` returns plausible top skills.
6. All tests pass; no real network calls in tests.
7. No personal data in the repository.

## Suggested order of work

1. Solution skeleton, AppHost, ServiceDefaults, PostgreSQL, empty API with health check.
2. Domain, EF Core model, first migration.
3. Source client + ingestion worker + upsert tests.
4. Extraction worker with a fake extractor first, then the real Ollama extractor.
5. API endpoints + integration tests.
6. Web UI (Razor Pages + htmx).
7. README, ADRs, cleanup.

Commit after each step.
