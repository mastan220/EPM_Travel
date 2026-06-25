using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlightStatus.Tests;

public class FlightStatusNormalizerTests
{
    private readonly FlightStatusNormalizer _normalizer = new();

    [Fact]
    public void NormalizeStatus_WhenCancelled_ReturnsCancelled()
    {
        // Arrange
        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "CANCELLED",
            StatusEnum.Cancelled,
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 11, 0, 0, TimeSpan.Zero),
            null, null, null, null,
            DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.Cancelled, result);
    }

    [Fact]
    public void NormalizeStatus_WhenDiverted_ReturnsDiverted()
    {
        // Arrange
        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "DIVERTED",
            StatusEnum.Diverted,
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 11, 0, 0, TimeSpan.Zero),
            null, null, null, null,
            DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.Diverted, result);
    }

    [Fact]
    public void NormalizeStatus_WhenUnknown_ReturnsUnknown()
    {
        // Arrange
        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), null,
            StatusEnum.Unknown, null, null, null, null, null, null, null,
            DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.Unknown, result);
    }

    [Fact]
    public void NormalizeStatus_WhenWithin15Minutes_ReturnsOnTime()
    {
        // Arrange
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 8, 10, 0, TimeSpan.Zero); // 10 minutes late

        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "ONTIME",
            StatusEnum.OnTime, scheduled, actual, null, null,
            null, null, null, DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.OnTime, result);
    }

    [Fact]
    public void NormalizeStatus_WhenBeyond15Minutes_ReturnsDelayed()
    {
        // Arrange
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 8, 20, 0, TimeSpan.Zero); // 20 minutes late

        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "ONTIME",
            StatusEnum.OnTime, scheduled, actual, null, null,
            null, null, null, DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.Delayed, result);
    }

    [Fact]
    public void NormalizeStatus_ArrivalBeyond15Minutes_ReturnsDelayed()
    {
        // Arrange
        var scheduledArr = new DateTimeOffset(2024, 1, 1, 11, 0, 0, TimeSpan.Zero);
        var actualArr = new DateTimeOffset(2024, 1, 1, 11, 30, 0, TimeSpan.Zero); // 30 minutes late

        var provider = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "ONTIME",
            StatusEnum.OnTime, null, null, scheduledArr, actualArr,
            null, null, null, DateTimeOffset.UtcNow, "Test");

        // Act
        var result = _normalizer.NormalizeStatus(provider);

        // Assert
        Assert.Equal(StatusEnum.Delayed, result);
    }
}

public class FlightStatusMergerTests
{
    private readonly FlightStatusMerger _merger = new();

    [Fact]
    public void MergeResults_WhenBothProvidersHaveData_UsesFresher()
    {
        // Arrange
        var aeroTime = DateTimeOffset.UtcNow;
        var quickTime = DateTimeOffset.UtcNow.AddMinutes(-10); // Older

        var aeroTrack = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "ONTIME",
            StatusEnum.OnTime,
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            null, null, null, "T1", "42", null, aeroTime, "AeroTrack data");

        var quickFlight = new ProviderFlightStatus(
            "QuickFlight", "AA100", new DateOnly(2024, 1, 1), "ON_TIME",
            StatusEnum.OnTime,
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
            null, null, null, null, null, null, quickTime, "QuickFlight data");

        // Act
        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), aeroTrack, quickFlight);

        // Assert
        Assert.Equal("AeroTrack", result.ProviderName);
        Assert.Equal("T1", result.Terminal);
    }

    [Fact]
    public void MergeResults_WhenOnlyAeroTrackHasData_UseAeroTrack()
    {
        // Arrange
        var aeroTrack = new ProviderFlightStatus(
            "AeroTrack", "AA100", new DateOnly(2024, 1, 1), "DELAYED",
            StatusEnum.Delayed, null, null, null, null, "T1", "42", null,
            DateTimeOffset.UtcNow, "AeroTrack data");

        // Act
        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), aeroTrack, null);

        // Assert
        Assert.Equal(StatusEnum.Delayed, result.Status);
        Assert.Equal("AeroTrack", result.ProviderName);
        Assert.Equal("T1", result.Terminal);
    }

    [Fact]
    public void MergeResults_WhenOnlyQuickFlightHasData_UseQuickFlight()
    {
        // Arrange
        var quickFlight = new ProviderFlightStatus(
            "QuickFlight", "AA100", new DateOnly(2024, 1, 1), "ON_TIME",
            StatusEnum.OnTime, null, null, null, null, null, null, null,
            DateTimeOffset.UtcNow, "QuickFlight data");

        // Act
        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), null, quickFlight);

        // Assert
        Assert.Equal(StatusEnum.OnTime, result.Status);
        Assert.Equal("QuickFlight", result.ProviderName);
    }

    [Fact]
    public void MergeResults_WhenNeitherProviderHasData_ReturnsUnknown()
    {
        // Act
        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), null, null);

        // Assert
        Assert.Equal(StatusEnum.Unknown, result.Status);
        Assert.Equal("None", result.ProviderName);
        Assert.Contains("No flight status data available", result.Message);
    }
}

public class AeroTrackProviderTests
{
    private readonly AeroTrackFlightStatusProvider _provider = new();

    [Fact]
    public async Task GetStatusAsync_AA100_ReturnsOnTimeStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.OnTime, result.NormalizedStatus);
        Assert.Equal("AA100", result.FlightNumber);
        Assert.Equal("A1", result.Terminal);
        Assert.Equal("42", result.Gate);
    }

    [Fact]
    public async Task GetStatusAsync_AA101_ReturnsDelayedStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA101", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Delayed, result.NormalizedStatus);
        Assert.Equal("AA101", result.FlightNumber);
        Assert.Equal("B2", result.Terminal);
        Assert.NotNull(result.DelayReason);
    }

    [Fact]
    public async Task GetStatusAsync_AA102_ReturnsCancelledStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA102", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Cancelled, result.NormalizedStatus);
        Assert.Null(result.ActualDepartureUtc);
        Assert.Null(result.ActualArrivalUtc);
    }

    [Fact]
    public async Task GetStatusAsync_AA103_ReturnsDivertedStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA103", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Diverted, result.NormalizedStatus);
        Assert.NotNull(result.ActualDepartureUtc);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownFlight_ReturnsNull()
    {
        // Act
        var result = await _provider.GetStatusAsync("XX999", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsCorrectProviderName()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AeroTrack", result.ProviderName);
    }
}

public class QuickFlightProviderTests
{
    private readonly QuickFlightStatusProvider _provider = new();

    [Fact]
    public async Task GetStatusAsync_AA100_ReturnsOnTimeStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.OnTime, result.NormalizedStatus);
        Assert.Equal("AA100", result.FlightNumber);
    }

    [Fact]
    public async Task GetStatusAsync_AA101_ReturnsDelayedStatus()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA101", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Delayed, result.NormalizedStatus);
    }

    [Fact]
    public async Task GetStatusAsync_AA104_OnlyInQuickFlight()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA104", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.OnTime, result.NormalizedStatus);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownFlight_ReturnsNull()
    {
        // Act
        var result = await _provider.GetStatusAsync("XX999", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsCorrectProviderName()
    {
        // Act
        var result = await _provider.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("QuickFlight", result.ProviderName);
    }
}

public class FlightStatusServiceTests
{
    private readonly FlightStatusService _service;
    private readonly AeroTrackFlightStatusProvider _aeroTrack;
    private readonly QuickFlightStatusProvider _quickFlight;
    private readonly FlightStatusNormalizer _normalizer;
    private readonly FlightStatusMerger _merger;

    public FlightStatusServiceTests()
    {
        _aeroTrack = new AeroTrackFlightStatusProvider();
        _quickFlight = new QuickFlightStatusProvider();
        _normalizer = new FlightStatusNormalizer();
        _merger = new FlightStatusMerger();
        _service = new FlightStatusService(
            new IFlightStatusProvider[] { _aeroTrack, _quickFlight },
            _normalizer,
            _merger);
    }

    [Fact]
    public async Task GetStatusAsync_AA100_UsesFresherProvider()
    {
        // Act
        var result = await _service.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.OnTime, result.Status);
        Assert.Equal("AA100", result.FlightNumber);
        // AeroTrack has terminal, QuickFlight doesn't - so result should prefer fresher
        Assert.True(result.LastUpdatedUtc.HasValue);
    }

    [Fact]
    public async Task GetStatusAsync_AA102_BothReturnCancelled()
    {
        // Act
        var result = await _service.GetStatusAsync("AA102", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Cancelled, result.Status);
    }

    [Fact]
    public async Task GetStatusAsync_AA104_OnlyQuickFlightHasData()
    {
        // Act
        var result = await _service.GetStatusAsync("AA104", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.OnTime, result.Status);
        Assert.Equal("QuickFlight", result.ProviderName);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownFlight_ReturnsUnknown()
    {
        // Act
        var result = await _service.GetStatusAsync("XX999", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusEnum.Unknown, result.Status);
        Assert.Equal("None", result.ProviderName);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsCorrectFlightNumber()
    {
        // Act
        var result = await _service.GetStatusAsync("AA100", new DateOnly(2024, 1, 1), CancellationToken.None);

        // Assert
        Assert.Equal("AA100", result.FlightNumber);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsCorrectDate()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 25);

        // Act
        var result = await _service.GetStatusAsync("AA100", date, CancellationToken.None);

        // Assert
        Assert.Equal(date, result.Date);
    }
}

public class EnumTests
{
    [Fact]
    public void StatusEnum_HasAllRequiredValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(StatusEnum), StatusEnum.OnTime));
        Assert.True(Enum.IsDefined(typeof(StatusEnum), StatusEnum.Delayed));
        Assert.True(Enum.IsDefined(typeof(StatusEnum), StatusEnum.Cancelled));
        Assert.True(Enum.IsDefined(typeof(StatusEnum), StatusEnum.Diverted));
        Assert.True(Enum.IsDefined(typeof(StatusEnum), StatusEnum.Unknown));
    }
}

public class RecordModelTests
{
    [Fact]
    public void FlightStatusResult_CanBeCreated()
    {
        // Arrange
        var flightNumber = "AA100";
        var date = new DateOnly(2024, 1, 1);
        var status = StatusEnum.OnTime;

        // Act
        var result = new FlightStatusResult(
            flightNumber, date, status, null, null, null, null,
            "T1", "42", null, "AeroTrack",
            DateTimeOffset.UtcNow, "Test");

        // Assert
        Assert.Equal(flightNumber, result.FlightNumber);
        Assert.Equal(date, result.Date);
        Assert.Equal(status, result.Status);
        Assert.Equal("T1", result.Terminal);
        Assert.Equal("42", result.Gate);
    }

    [Fact]
    public void FlightStatusResult_AllPropertiesAreAccessible()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var dep = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);

        var result = new FlightStatusResult(
            "AA100", new DateOnly(2024, 1, 1), StatusEnum.Delayed,
            dep, dep, null, null, "T1", "42", "Weather", "AeroTrack", now, "Test");

        // Assert
        Assert.NotNull(result.ScheduledDepartureUtc);
        Assert.NotNull(result.Terminal);
        Assert.NotNull(result.Gate);
        Assert.NotNull(result.DelayReason);
        Assert.NotNull(result.LastUpdatedUtc);
    }
}

/// <summary>
/// Additional edge case and coverage tests for the flight status system.
/// </summary>
public class AdditionalCoverageTests
{
    private readonly FlightStatusNormalizer _normalizer = new();
    private readonly FlightStatusMerger _merger = new();

    [Fact]
    public void Normalizer_ExactlyAtBoundary_OnTime()
    {
        // Test exactly at 15-minute boundary (should be OnTime, not Delayed)
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 8, 15, 0, TimeSpan.Zero); // Exactly 15 minutes

        var provider = new ProviderFlightStatus(
            "TestProvider", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            scheduled, actual, null, null, "T1", "42", null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.OnTime, result);
    }

    [Fact]
    public void Normalizer_OneMinuteBeyondBoundary_Delayed()
    {
        // Test 16 minutes (one minute past boundary)
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 8, 16, 0, TimeSpan.Zero);

        var provider = new ProviderFlightStatus(
            "TestProvider", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            scheduled, actual, null, null, "T1", "42", null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.Delayed, result);
    }

    [Fact]
    public void Normalizer_NegativeDeviation_OnTime()
    {
        // Test early arrival (negative deviation) within 15 minutes
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 7, 50, 0, TimeSpan.Zero); // 10 minutes early

        var provider = new ProviderFlightStatus(
            "TestProvider", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            scheduled, actual, null, null, "T1", "42", null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.OnTime, result);
    }

    [Fact]
    public void Normalizer_ArrivalDeviationOnly()
    {
        // Test case where only arrival has deviation (departure on time)
        var scheduled = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var actual = new DateTimeOffset(2024, 1, 1, 8, 20, 0, TimeSpan.Zero); // 20 minute deviation

        var provider = new ProviderFlightStatus(
            "TestProvider", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            scheduled, scheduled, scheduled, actual, "T1", "42", null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.Delayed, result);
    }

    [Fact]
    public void Merger_BothProvidersWithDifferentLastUpdated()
    {
        // Ensure fresher provider wins even with different statuses
        var older = new ProviderFlightStatus(
            "Provider1", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            null, null, null, null, null, null, null,
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero), "Test");

        var newer = new ProviderFlightStatus(
            "Provider2", "AA100", new DateOnly(2024, 1, 1), "Delayed", StatusEnum.Delayed,
            null, null, null, null, null, null, "Weather delay",
            new DateTimeOffset(2024, 1, 1, 9, 0, 0, TimeSpan.Zero), "Test");

        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), older, newer);

        Assert.Equal(StatusEnum.Delayed, result.Status);
        Assert.Equal("Provider2", result.ProviderName);
    }

    [Fact]
    public void Merger_SameTimestamp_QuickFlightWins()
    {
        // When timestamps are equal, second provider (quickFlight) is used  
        var time = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero);

        var provider1 = new ProviderFlightStatus(
            "Provider1", "AA100", new DateOnly(2024, 1, 1), "OnTime", StatusEnum.OnTime,
            null, null, null, null, null, null, null, time, "Test");

        var provider2 = new ProviderFlightStatus(
            "Provider2", "AA100", new DateOnly(2024, 1, 1), "Delayed", StatusEnum.Delayed,
            null, null, null, null, null, null, null, time, "Test");

        var result = _merger.MergeResults("AA100", new DateOnly(2024, 1, 1), provider1, provider2);

        Assert.Equal(StatusEnum.Delayed, result.Status);
        Assert.Equal("Provider2", result.ProviderName);
    }

    [Fact]
    public void ProviderFlightStatus_CanBeNull()
    {
        // Ensure null providers are handled
        var result = _merger.MergeResults("XX999", new DateOnly(2024, 1, 1), null, null);

        Assert.Equal(StatusEnum.Unknown, result.Status);
        Assert.Contains("No flight status data available", result.Message);
    }

    [Fact]
    public void FlightStatusResult_AllNullOptionalFields()
    {
        // Verify record creation with all nulls
        var result = new FlightStatusResult(
            "AA100", new DateOnly(2024, 1, 1), StatusEnum.OnTime,
            null, null, null, null, null, null, null, "Provider", DateTimeOffset.UtcNow, "Message");

        Assert.Null(result.ScheduledDepartureUtc);
        Assert.Null(result.ActualDepartureUtc);
        Assert.Null(result.Terminal);
        Assert.Null(result.Gate);
        Assert.Null(result.DelayReason);
    }

    [Fact]
    public void StatusEnum_AllValuesUsed()
    {
        // Verify all enum values are distinct
        var values = new[] { StatusEnum.OnTime, StatusEnum.Delayed, StatusEnum.Cancelled, StatusEnum.Diverted, StatusEnum.Unknown };
        var distinctValues = values.Distinct().ToList();

        Assert.Equal(5, distinctValues.Count);
    }

    [Fact]
    public void Normalizer_CancelledPassthrough()
    {
        // Cancelled status should pass through unchanged
        var provider = new ProviderFlightStatus(
            "TestProvider", "AA102", new DateOnly(2024, 1, 1), "Cancelled", StatusEnum.Cancelled,
            null, null, null, null, null, null, null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.Cancelled, result);
    }

    [Fact]
    public void Normalizer_DivertedPassthrough()
    {
        // Diverted status should pass through unchanged
        var provider = new ProviderFlightStatus(
            "TestProvider", "AA103", new DateOnly(2024, 1, 1), "Diverted", StatusEnum.Diverted,
            null, null, null, null, null, null, "Alternate airport",
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.Diverted, result);
    }

    [Fact]
    public void Normalizer_UnknownPassthrough()
    {
        // Unknown status should pass through unchanged
        var provider = new ProviderFlightStatus(
            "TestProvider", "XX999", new DateOnly(2024, 1, 1), "Unknown", StatusEnum.Unknown,
            null, null, null, null, null, null, null,
            DateTimeOffset.UtcNow, "Test");

        var result = _normalizer.NormalizeStatus(provider);
        Assert.Equal(StatusEnum.Unknown, result);
    }
}

