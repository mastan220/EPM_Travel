using FlightStatus.Api.Models;

namespace FlightStatus.Api.Providers;

public interface IFlightStatusProvider
{
    string Name { get; }

    Task<ProviderFlightStatus?> GetStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken);
}
