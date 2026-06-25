using FlightStatus.Api.Models;

namespace FlightStatus.Api.Services;

public class FlightStatusNormalizer
{
    /// <summary>
    /// Normalizes a provider result into the unified StatusEnum enum,
    /// applying the 15-minute rule for OnTime vs Delayed.
    /// </summary>
    public StatusEnum NormalizeStatus(ProviderFlightStatus provider)
    {
        if (provider.NormalizedStatus == StatusEnum.Cancelled ||
            provider.NormalizedStatus == StatusEnum.Diverted)
        {
            return provider.NormalizedStatus;
        }

        // Apply 15-minute rule for OnTime vs Delayed
        if (provider.NormalizedStatus == StatusEnum.Unknown)
        {
            return StatusEnum.Unknown;
        }

        // Check departure
        if (provider.ScheduledDepartureUtc.HasValue && provider.ActualDepartureUtc.HasValue)
        {
            var departureDeviation = Math.Abs((provider.ActualDepartureUtc.Value - provider.ScheduledDepartureUtc.Value).TotalMinutes);
            if (departureDeviation > 15)
            {
                return StatusEnum.Delayed;
            }
        }

        // Check arrival
        if (provider.ScheduledArrivalUtc.HasValue && provider.ActualArrivalUtc.HasValue)
        {
            var arrivalDeviation = Math.Abs((provider.ActualArrivalUtc.Value - provider.ScheduledArrivalUtc.Value).TotalMinutes);
            if (arrivalDeviation > 15)
            {
                return StatusEnum.Delayed;
            }
        }

        return StatusEnum.OnTime;
    }
}
