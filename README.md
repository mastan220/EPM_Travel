# Flight Status Tracker

A local, offline Flight Status lookup application for the SkyRoute platform.

A support agent enters a flight number and date. The backend queries two deterministic stub providers, normalises their responses into a unified status model, merges them using freshness rules, and returns a single `FlightStatusResult`. The frontend displays the result with clear status colour coding and error states.

## Tech Stack

- Backend: .NET 10 Minimal API, C#
- Frontend: React with Vite and TypeScript
- Tests: xUnit + coverlet collector
- AI tooling: IDE-integrated coding assistant such as GitHub Copilot, Cursor, or Claude Code

> Note: The challenge allows .NET 8+. This solution targets the latest LTS .NET version available at project start. If your interview environment only supports .NET 8, change `TargetFramework` from `net10.0` to `net8.0`.

## Repository Structure

```text
flight-status/
├── README.md
├── spec.md
├── prompts.md
├── reflection.md
├── .github/
│   └── copilot-instructions.md
├── docs/
│   ├── implementation-plan.md
│   ├── development-agent.md
│   ├── unit-test-agent.md
│   └── code-review-agent.md
├── FlightStatus.Api/
├── FlightStatus.Tests/
└── flight-status-ui/
```

## Prerequisites

Install the following before running locally:

- .NET SDK 10.x or .NET SDK 8.x
- Node.js LTS
- Git
- IDE-integrated AI tool enabled in VS Code or your preferred IDE

## Setup Commands

```bash
git clone <your-public-github-repo-url>
cd flight-status
```

## Backend Setup

```bash
dotnet new sln -n FlightStatus

dotnet new webapi -n FlightStatus.Api --framework net10.0
dotnet new xunit -n FlightStatus.Tests --framework net10.0

dotnet sln add FlightStatus.Api/FlightStatus.Api.csproj
dotnet sln add FlightStatus.Tests/FlightStatus.Tests.csproj

dotnet add FlightStatus.Tests/FlightStatus.Tests.csproj reference FlightStatus.Api/FlightStatus.Api.csproj

dotnet add FlightStatus.Tests/FlightStatus.Tests.csproj package coverlet.collector
dotnet add FlightStatus.Tests/FlightStatus.Tests.csproj package FluentAssertions
```

If you use .NET 8 instead:

```bash
# Replace net10.0 with net8.0 in both project files.
```

## Frontend Setup

```bash
npm create vite@latest flight-status-ui -- --template react-ts
cd flight-status-ui
npm install
cd ..
```

## Run Backend

```bash
dotnet run --project FlightStatus.Api
```

Expected endpoint:

```http
GET /flights/status?flightNumber=SR101&date=2026-06-25
```

## Run Frontend

```bash
cd flight-status-ui
npm run dev
```

## Run Tests with Coverage

```bash
dotnet test FlightStatus.Tests/FlightStatus.Tests.csproj --collect:"XPlat Code Coverage"
```

Optional coverage report:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## Assumptions

- Providers are deterministic stubs and do not call external APIs.
- `flightNumber` is treated case-insensitively and trimmed.
- `date` must be in `yyyy-MM-dd` format.
- AeroTrack can return optional enrichment fields: terminal, gate, and delay reason.
- QuickFlight returns minimal schedule/status fields only.
- If both providers respond, the provider result with later `lastUpdatedUtc` wins.
- If no provider returns usable data, the result status is `Unknown` with a clear message.

## Demo Checklist

- Search valid flight returning `OnTime`.
- Search valid flight returning `Delayed`.
- Search valid flight returning `Cancelled`.
- Search valid flight returning `Diverted`.
- Search unknown flight returning `Unknown`.
- Call API without `flightNumber` and verify HTTP 400.
- Call API without `date` and verify HTTP 400.
- Show test coverage report targeting 90%+ coverage.
- Explain AI prompts and decisions from `prompts.md`.
