using FlightStatus.Api.Models;

namespace FlightStatus.Api.Services;

public class FlightStatusMerger
{
    /// <summary>
    /// Merges two provider results into a single FlightStatusResult.
    /// Rules:
    /// - Later LastUpdatedUtc wins when both providers return a result.
    /// - Single provider result is used if only one responds.
    /// - Unknown result is returned when neither responds.
    /// </summary>
    public FlightStatusResult MergeResults(
        string flightNumber,
        DateOnly date,
        ProviderFlightStatus? aeroTrack,
        ProviderFlightStatus? quickFlight)
    {
        // Both providers have data
        if (aeroTrack != null && quickFlight != null)
        {
            var aeroTime = aeroTrack.LastUpdatedUtc ?? DateTimeOffset.MinValue;
            var quickTime = quickFlight.LastUpdatedUtc ?? DateTimeOffset.MinValue;

            var winner = aeroTime > quickTime ? aeroTrack : quickFlight;
            return ProviderToResult(winner, flightNumber, date);
        }

        // Only one provider has data
        if (aeroTrack != null)
        {
            return ProviderToResult(aeroTrack, flightNumber, date);
        }

        if (quickFlight != null)
        {
            return ProviderToResult(quickFlight, flightNumber, date);
        }

        // Neither provider has data
        return new FlightStatusResult(
            FlightNumber: flightNumber,
            Date: date,
            Status: StatusEnum.Unknown,
            ScheduledDepartureUtc: null,
            ActualDepartureUtc: null,
            ScheduledArrivalUtc: null,
            ActualArrivalUtc: null,
            Terminal: null,
            Gate: null,
            DelayReason: null,
            ProviderName: "None",
            LastUpdatedUtc: null,
            Message: "No flight status data available from any provider."
        );
    }

    private static FlightStatusResult ProviderToResult(
        ProviderFlightStatus provider,
        string flightNumber,
        DateOnly date)
    {
        return new FlightStatusResult(
            FlightNumber: flightNumber,
            Date: date,
            Status: provider.NormalizedStatus,
            ScheduledDepartureUtc: provider.ScheduledDepartureUtc,
            ActualDepartureUtc: provider.ActualDepartureUtc,
            ScheduledArrivalUtc: provider.ScheduledArrivalUtc,
            ActualArrivalUtc: provider.ActualArrivalUtc,
            Terminal: provider.Terminal,
            Gate: provider.Gate,
            DelayReason: provider.DelayReason,
            ProviderName: provider.ProviderName,
            LastUpdatedUtc: provider.LastUpdatedUtc,
            Message: provider.Message
        );
    }
}
