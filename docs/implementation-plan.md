# Implementation Plan

## Phase 1 — Documentation First

1. Create repository folder `flight-status`.
2. Add `README.md`, `spec.md`, `prompts.md`, and `reflection.md`.
3. Commit `spec.md` before implementation.

Suggested first commit:

```bash
git init
git add spec.md README.md prompts.md reflection.md docs .github
git commit -m "docs: add challenge specification and implementation plan"
```

## Phase 2 — Backend

1. Create solution and projects.
2. Add models:
   - `FlightStatus`
   - `ProviderFlightStatus`
   - `FlightStatusResult`
3. Add provider abstraction:
   - `IFlightStatusProvider`
4. Add deterministic providers:
   - `AeroTrackFlightStatusProvider`
   - `QuickFlightStatusProvider`
5. Add business logic:
   - `FlightStatusNormalizer`
   - `FlightStatusMerger`
   - `FlightStatusService`
6. Add Minimal API endpoint:
   - `GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}`

## Phase 3 — Tests

Target at least 90% coverage through meaningful unit tests.

Test categories:

- Normalisation rules.
- Merge rules.
- Service provider coordination.
- API validation.
- API success responses.

## Phase 4 — Frontend

1. Create React + TypeScript app using Vite.
2. Build search form.
3. Call backend API.
4. Render loading, result, empty, and error states.
5. Apply status colour mapping.
6. Show optional AeroTrack-only fields conditionally.

## Phase 5 — Review

Use `docs/code-review-agent.md` to review:

- Architecture.
- Challenge compliance.
- Tests and coverage.
- Documentation completeness.
- Clean clone run path.
- Secrets check.

## Phase 6 — Submission

1. Run app locally.
2. Run tests and coverage.
3. Update `reflection.md` with honest improvements.
4. Push to public GitHub repository.
5. Demo from clean clone.
