# AI Prompts and Decisions Log

This document captures significant prompts and key judgement calls used while building the Flight Status Tracker.

## Prompt 1 — Requirements Analysis

```text
Analyze the attached Flight Status Tracker challenge. Extract functional requirements, non-functional requirements, data models, API contract, merge rules, frontend states, test strategy, and submission checklist. Produce spec.md first. Do not generate implementation code yet.
```

### Decision Notes

- I created `spec.md` before implementation to satisfy the challenge requirement.
- I chose a provider-based architecture using `IFlightStatusProvider` to keep AeroTrack and QuickFlight isolated.
- I kept providers deterministic and local-only because the challenge excludes real APIs, credentials, auth, and persistence.

---

## Prompt 2 — Backend Architecture

```text
Using .NET 10 Minimal API, design a clean backend for Flight Status Tracker. Implement provider abstraction, AeroTrack and QuickFlight deterministic stubs, normalisation logic, merge logic, service layer, and GET /flights/status endpoint. Keep business logic separate from Program.cs so xUnit tests can cover it easily.
```

### Decision Notes

- I separated normalisation and merge logic from the endpoint to make tests meaningful.
- I used records for immutable request/response models.
- I avoided external dependencies except test/coverage packages.

---

## Prompt 3 — Test Generation

```text
Create xUnit tests for FlightStatusNormalizer, FlightStatusMerger, FlightStatusService, and Minimal API validation. Tests must be meaningful, cover edge cases, and support at least 90% line/branch coverage. Include examples for OnTime, Delayed, Cancelled, Diverted, Unknown, missing query params, single provider response, and latest lastUpdatedUtc merge rule.
```

### Decision Notes

- I prioritised core business logic coverage over cosmetic endpoint-only tests.
- I added edge cases for null provider responses and invalid/missing inputs.

---

## Prompt 4 — Frontend Implementation

```text
Build a React + TypeScript frontend for Flight Status Tracker. Create a search form for flight number and date, call the backend endpoint, render a result card with status colour coding, show AeroTrack-only fields only when present, and show clear loading/error/empty states.
```

### Decision Notes

- I chose React + Vite because it is quick to run locally and easy to demo.
- I kept the UI simple to support live interview change tasks.

---

## Prompt 5 — Code Review

```text
Review this Flight Status Tracker solution against the challenge specification. Check architecture, API validation, provider abstraction, deterministic stubs, normalisation rules, merge rules, frontend states, meaningful tests, 90% coverage readiness, README completeness, prompts.md honesty, reflection.md quality, and absence of secrets. Return a prioritized fix list with file names and exact changes.
```

### Decision Notes

- I used review prompts after implementation rather than blindly accepting generated code.
- I checked that documentation and demo instructions work from a clean clone.

---

## Prompt 6 — Final Submission Check

```text
Act as a strict evaluator. Verify the repository against the challenge Definition of Done. Confirm spec.md was committed before implementation files, all required folders/files exist, API behaviour matches requirements, frontend states are implemented, unit tests are meaningful, coverage is 90%+, README can run the app from clean clone, and no secrets are committed.
```

### Decision Notes

- I used this to identify final gaps before pushing to the public GitHub repository.
