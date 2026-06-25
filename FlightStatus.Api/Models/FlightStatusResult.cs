namespace FlightStatus.Api.Models;

public sealed record FlightStatusResult(
    string FlightNumber,
    DateOnly Date,
    StatusEnum Status,
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
