using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;

namespace FlightStatus.Api.Services;

public class FlightStatusService
{
    private readonly IFlightStatusProvider[] _providers;
    private readonly FlightStatusNormalizer _normalizer;
    private readonly FlightStatusMerger _merger;

    public FlightStatusService(
        IEnumerable<IFlightStatusProvider> providers,
        FlightStatusNormalizer normalizer,
        FlightStatusMerger merger)
    {
        _providers = providers.ToArray();
        _normalizer = normalizer;
        _merger = merger;
    }

    /// <summary>
    /// Gets the merged flight status from all providers.
    /// </summary>
    public async Task<FlightStatusResult> GetStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Query all providers in parallel
        var tasks = _providers.Select(p =>
            p.GetStatusAsync(flightNumber, date, cancellationToken)
        ).ToList();

        var results = await Task.WhenAll(tasks);

        // Get the original results with normalized status applied
        var aeroTrack = results.FirstOrDefault(r => r?.ProviderName == "AeroTrack");
        var quickFlight = results.FirstOrDefault(r => r?.ProviderName == "QuickFlight");

        // Apply normalized status
        if (aeroTrack != null)
        {
            aeroTrack = aeroTrack with { NormalizedStatus = _normalizer.NormalizeStatus(aeroTrack) };
        }

        if (quickFlight != null)
        {
            quickFlight = quickFlight with { NormalizedStatus = _normalizer.NormalizeStatus(quickFlight) };
        }

        return _merger.MergeResults(flightNumber, date, aeroTrack, quickFlight);
    }
}
