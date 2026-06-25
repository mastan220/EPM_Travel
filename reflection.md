# Reflection

## What Went Well

- The solution was designed before coding by creating `spec.md` first.
- Provider logic was abstracted behind `IFlightStatusProvider`, making it easy to add more providers later.
- Normalisation and merge rules were isolated from API routing, making them easier to test.
- Deterministic stubs allowed the app to run fully offline and made demos repeatable.
- Tests focused on business rules such as status mapping, latest-update merge behaviour, provider fallback, and validation.

## Key Design Decisions

- I selected .NET Minimal API for a lightweight backend suitable for a coding challenge and live-change tasks.
- I selected React + TypeScript for a simple UI with clear states and fast local startup.
- I used immutable records for contracts to keep API models predictable.
- I kept provider-specific raw data out of the frontend except for optional enriched fields such as gate, terminal, and delay reason.

## AI Tooling Usage

AI was used across the SDLC:

- Requirements analysis and extraction.
- Initial specification drafting.
- Backend architecture planning.
- Test case generation support.
- Code review checklist creation.
- Documentation refinement.

AI output was reviewed manually, especially around merge rules, deterministic stub cases, and test coverage. I did not treat AI-generated code as automatically correct.

## What I Would Improve With More Time

- Add integration tests using `WebApplicationFactory` for full API verification.
- Add frontend component tests for status colours, conditional fields, and error states.
- Add OpenAPI examples for each deterministic flight scenario.
- Add structured logging for provider selection and merge decisions.
- Add a small in-app demo data panel listing supported stub flight numbers.
- Add branch coverage thresholds directly in CI.
- Add GitHub Actions to build backend, run tests, collect coverage, and build frontend.

## Known Constraints

- Providers are deterministic stubs only.
- No persistence, authentication, or real API integration is included because the challenge explicitly excludes those areas.
- The app is optimised for clarity, correctness, and demo readiness rather than production-grade operations.
