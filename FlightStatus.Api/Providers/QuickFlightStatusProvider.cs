using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

public class QuickFlightStatusProvider : IFlightStatusProvider
{
    public string Name => "QuickFlight";

    public Task<ProviderFlightStatus?> GetStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // Deterministic stub data - different from AeroTrack for some flights
        ProviderFlightStatus? result = flightNumber.ToUpper() switch
        {
            "AA100" => new ProviderFlightStatus(
                Name, flightNumber, date, "ON_TIME", StatusEnum.OnTime,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(8, 1)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(11, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(11, 3)), TimeSpan.Zero),
                null, null, null,
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(2, 15)), $"Data from {Name}"),

            "AA101" => new ProviderFlightStatus(
                Name, flightNumber, date, "LATE", StatusEnum.Delayed,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(10, 50)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(13, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(13, 50)), TimeSpan.Zero),
                null, null, null,
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(2, 15)), $"Data from {Name}"),

            "AA102" => new ProviderFlightStatus(
                Name, flightNumber, date, "CANCELLED", StatusEnum.Cancelled,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(14, 0)), TimeSpan.Zero),
                null,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(17, 0)), TimeSpan.Zero),
                null,
                null, null, null,
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(2, 15)), $"Data from {Name}"),

            "AA104" => new ProviderFlightStatus(
                Name, flightNumber, date, "ON_TIME", StatusEnum.OnTime,
                new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 5)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(15, 0)), TimeSpan.Zero),
                new DateTimeOffset(date.ToDateTime(new TimeOnly(15, 10)), TimeSpan.Zero),
                null, null, null,
                DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(2, 15)), $"Data from {Name}"),

            _ => null
        };

        return Task.FromResult(result);
    }
}
