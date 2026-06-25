# Copilot Instructions — Flight Status Tracker

## Project Goal

Build a local offline Flight Status Tracker for the SkyRoute challenge using .NET Minimal API, React + TypeScript, and xUnit tests.

## Mandatory Requirements

- Use `IFlightStatusProvider` abstraction.
- Implement two DI-injected deterministic stubs:
  - `AeroTrackFlightStatusProvider`
  - `QuickFlightStatusProvider`
- Do not call real APIs.
- Do not add credentials, secrets, auth, persistence, or external service dependencies.
- Store core business logic outside `Program.cs`.
- Keep code readable for live interview changes.

## Backend Rules

Endpoint:

```http
GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}
```

Validation:

- Missing/blank `flightNumber` returns 400.
- Missing/invalid `date` returns 400.

Status enum:

```csharp
OnTime, Delayed, Cancelled, Diverted, Unknown
```

Normalisation:

- OnTime: departure or arrival within 15 minutes of schedule.
- Delayed: departure or arrival beyond 15 minutes.
- Cancelled: flight will not operate.
- Diverted: flight landed at a different airport.
- Unknown: no usable status.

Merge:

- Later `lastUpdatedUtc` wins when both providers return data.
- Single provider result is used.
- Unknown result is returned when neither provider has usable data.

## Frontend Rules

- Use flight number + date search form.
- Render result card.
- Colour mapping:
  - OnTime: green
  - Delayed: amber
  - Cancelled/Diverted: red
  - Unknown: grey
- Show terminal, gate, and delay reason only when present.
- Show clear error state when API fails.

## Test Rules

- Use xUnit.
- Target 90%+ meaningful coverage.
- Cover normalisation, merge, service coordination, provider fallback, unknown case, and API validation.

## Documentation Rules

- Keep README runnable from clean clone.
- Update prompts.md for significant AI prompts.
- Update reflection.md for final judgement calls and improvements.
