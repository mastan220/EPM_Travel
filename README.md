# Flight Status Tracker

A local, offline Flight Status lookup application for the SkyRoute platform.

A support agent enters a flight number and date. The backend queries two deterministic stub providers, normalises their responses into a unified status model, merges them using freshness rules, and returns a single `FlightStatusResult`. The frontend displays the result with clear status color coding and error states.

## Tech Stack

- Backend: .NET 8 Minimal API, C#
- Frontend: React + Vite + TypeScript
- Tests: xUnit + coverlet collector

## Prerequisites

Install the following before running locally:

- .NET SDK 8.x
- Node.js LTS (18+ recommended)
- Git

## Clone Repository

```bash
git clone https://github.com/mastan220/EPM_Travel.git
cd EPM_Travel
```

## Restore Dependencies

```bash
dotnet restore FlightStatus.sln
cd flight-status-ui
npm install
cd ..
```

## Run Backend API

```bash
dotnet run --project FlightStatus.Api
```

The API starts on:

- https://localhost:7070
- http://localhost:5219

Swagger UI:

- https://localhost:7070/swagger

## Run Frontend UI

Open a second terminal:

```bash
cd flight-status-ui
npm run dev
```

Vite runs on:

- http://localhost:5173

By default, the UI calls `https://localhost:7070`. To change backend URL, create `flight-status-ui/.env`:

```env
VITE_API_URL=https://localhost:7070
```

## Test the API Quickly

Example request:

```http
GET https://localhost:7070/flights/status?flightNumber=SR101&date=2026-06-25
```

PowerShell example:

```powershell
Invoke-RestMethod "https://localhost:7070/flights/status?flightNumber=SR101&date=2026-06-25"
```

## Run Tests

```bash
dotnet test FlightStatus.Tests/FlightStatus.Tests.csproj
```

Run tests with coverage:

```bash
dotnet test FlightStatus.Tests/FlightStatus.Tests.csproj --collect:"XPlat Code Coverage"
```

Optional HTML coverage report:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## Behavior Notes

- Providers are deterministic stubs and do not call external APIs.
- `flightNumber` is required and treated case-insensitively.
- `date` is required in `yyyy-MM-dd` format.
- Merge rule: when both providers return data, later `lastUpdatedUtc` wins.
- If no provider has usable data, status is `Unknown`.

## Common Issues

- HTTPS certificate warning on first run:

```bash
dotnet dev-certs https --trust
```

- Port conflict: stop the process using 7070 or 5173, then rerun.
