# Flight Status Tracker Specification

> This file should be committed before implementation files.

## Problem Summary

Build a Flight Status lookup feature for SkyRoute. A support agent enters a flight number and date. The system queries two deterministic stub providers, normalises the provider responses into a unified model, merges responses, and displays the final status.

## Functional Requirements

### API

```http
GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}
```

Rules:

- Return `400 Bad Request` when `flightNumber` is missing or blank.
- Return `400 Bad Request` when `date` is missing or invalid.
- Query both providers through the `IFlightStatusProvider` abstraction.
- Normalise provider-specific statuses into the unified status enum.
- Merge provider results into one `FlightStatusResult`.
- Run fully offline without real flight APIs, credentials, authentication, or persistence.

### Frontend

- Search form with flight number and date.
- Result card with status colour coding:
  - `OnTime`: green
  - `Delayed`: amber
  - `Cancelled`: red
  - `Diverted`: red
  - `Unknown`: grey
- Show AeroTrack-only fields only when present:
  - terminal
  - gate
  - delay reason
- Show clear error state when the API fails.

## Unified Status Enum

```csharp
public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
```

## Unified Result Contract

```csharp
public sealed record FlightStatusResult(
    string FlightNumber,
    DateOnly Date,
    FlightStatus Status,
    DateTimeOffset? ScheduledDepartureUtc,
    DateTimeOffset? ActualDepartureUtc,
    DateTimeOffset? ScheduledArrivalUtc,
    DateTimeOffset? ActualArrivalUtc,
    string? Terminal,
    string? Gate,
    string? DelayReason,
    string ProviderName,
    DateTimeOffset? LastUpdatedUtc,
    string Message
);
```

## Provider Contract

```csharp
public interface IFlightStatusProvider
{
    string Name { get; }

    Task<ProviderFlightStatus?> GetStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken);
}
```

## Normalised Provider Contract

```csharp
public sealed record ProviderFlightStatus(
    string ProviderName,
    string FlightNumber,
    DateOnly Date,
    string? RawStatus,
    FlightStatus NormalizedStatus,
    DateTimeOffset? ScheduledDepartureUtc,
    DateTimeOffset? ActualDepartureUtc,
    DateTimeOffset? ScheduledArrivalUtc,
    DateTimeOffset? ActualArrivalUtc,
    string? Terminal,
    string? Gate,
    string? DelayReason,
    DateTimeOffset LastUpdatedUtc,
    string Message
);
```

## Provider Capabilities

| Provider | Detail Level | Required Fields |
|---|---|---|
| AeroTrack | Full | status, scheduled times, actual times, terminal, gate, delay reason, `lastUpdatedUtc` |
| QuickFlight | Minimal | status, scheduled times, `lastUpdatedUtc` |

## Normalisation Rules

| Unified Status | Rule |
|---|---|
| `OnTime` | Departure or arrival is within 15 minutes of schedule. |
| `Delayed` | Departure or arrival is pushed beyond 15 minutes. |
| `Cancelled` | Flight will not operate. |
| `Diverted` | Flight landed at a different airport. |
| `Unknown` | No usable status is returned by either provider. |

## Merge Rules

1. If both providers return usable results, choose the result with later `lastUpdatedUtc`.
2. If only one provider returns a usable result, use that result.
3. If neither provider returns usable data, return `Unknown` with a clear message.

## Deterministic Stub Data Plan

| Flight Number | Scenario | Provider Behaviour |
|---|---|---|
| `SR101` | OnTime | Both providers return OnTime; latest update wins. |
| `SR202` | Delayed | AeroTrack returns full delayed details with gate/terminal/reason. |
| `SR303` | Cancelled | QuickFlight returns cancelled/latest result. |
| `SR404` | Diverted | AeroTrack returns diverted details. |
| `SR505` | Single provider only | Only one provider returns a result. |
| `SR000` or unknown | Unknown | Neither provider returns usable status. |

## Backend Components

| Component | Responsibility |
|---|---|
| `IFlightStatusProvider` | Provider abstraction. |
| `AeroTrackFlightStatusProvider` | Deterministic full-detail stub. |
| `QuickFlightStatusProvider` | Deterministic minimal-detail stub. |
| `FlightStatusNormalizer` | Converts provider raw statuses/times into unified enum. |
| `FlightStatusMerger` | Applies latest-update merge rule. |
| `FlightStatusService` | Coordinates providers, normalisation, and merge. |
| Minimal API endpoint | Validates query params and returns HTTP responses. |

## Test Strategy for 90% Coverage

### Unit Tests

- Normalisation tests:
  - OnTime when actual time is within 15 minutes.
  - Delayed when actual time is beyond 15 minutes.
  - Cancelled raw status maps correctly.
  - Diverted raw status maps correctly.
  - Missing/unknown status returns Unknown.

- Merge tests:
  - Later `lastUpdatedUtc` wins.
  - Single provider result is returned.
  - No provider result returns Unknown.

- Service tests:
  - Queries both providers.
  - Handles provider returning null.
  - Returns deterministic scenarios.

- API tests:
  - Missing flight number returns 400.
  - Missing date returns 400.
  - Valid request returns 200 and expected body.

### Frontend Tests, Optional

- Status colour mapping.
- Conditional AeroTrack fields.
- API error state rendering.

## Non-Functional Requirements

- No secrets or credentials.
- No persistence.
- Clean clone run path documented in README.
- Extensible provider structure.
- Simple, readable code suitable for live-change tasks.
