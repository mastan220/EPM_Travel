using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

public class AeroTrackFlightStatusProvider : IFlightStatusProvider
{
    public string Name => "AeroTrack";

    public Task<ProviderFlightStatus?> GetStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // Deterministic stub data based on flight number
        ProviderFlightStatus? result = flightNumber.ToUpper() switch
        {
            "AA100" => new ProviderFlightStatus(
                Name, flightNumber, date, "ONTIME", StatusEnum.OnTime,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(8, 2)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(11, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(11, 5)), TimeSpan.Zero),
                "A1", "42", null,
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(5, 30)), $"Data from {Name}"),

            "AA101" => new ProviderFlightStatus(
                Name, flightNumber, date, "DELAYED", StatusEnum.Delayed,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(10, 45)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(13, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(13, 45)), TimeSpan.Zero),
                "B2", "15", "Mechanical issue resolved",
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(5, 30)), $"Data from {Name}"),

            "AA102" => new ProviderFlightStatus(
                Name, flightNumber, date, "CANCELLED", StatusEnum.Cancelled,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(14, 0)), TimeSpan.Zero),
                null,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(17, 0)), TimeSpan.Zero),
                null,
                "C3", null, "Aircraft maintenance",
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(5, 30)), $"Data from {Name}"),

            "AA103" => new ProviderFlightStatus(
                Name, flightNumber, date, "DIVERTED", StatusEnum.Diverted,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(16, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(16, 15)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(19, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(19, 30)), TimeSpan.Zero),
                "D4", "8", "Weather diversion",
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(5, 30)), $"Data from {Name}"),

            _ => null
        };

        return Task.FromResult(result);
    }
}
