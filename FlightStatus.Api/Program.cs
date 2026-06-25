using FlightStatus.Api.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON serialization to convert enums to strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register business services
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightStatusProvider>();
builder.Services.AddScoped<FlightStatusNormalizer>();
builder.Services.AddScoped<FlightStatusMerger>();
builder.Services.AddScoped<FlightStatusService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Flight Status Endpoint
app.MapGet("/flights/status", async (
    string? flightNumber,
    string? date,
    FlightStatusService flightStatusService,
    CancellationToken cancellationToken) =>
{
    // Validate flightNumber
    if (string.IsNullOrWhiteSpace(flightNumber))
    {
        return Results.BadRequest(new { error = "flightNumber is required and cannot be blank" });
    }

    // Validate and parse date with strict format
    if (string.IsNullOrWhiteSpace(date))
    {
        return Results.BadRequest(new { error = "date is required in yyyy-MM-dd format" });
    }

    // Validate exact format: yyyy-MM-dd
    if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
    {
        return Results.BadRequest(new { error = "date must be in yyyy-MM-dd format" });
    }

    var result = await flightStatusService.GetStatusAsync(flightNumber, parsedDate, cancellationToken);
    return Results.Ok(result);
})
.WithName("GetFlightStatus")
.WithOpenApi();

app.Run();

// Making Program public partial for test WebApplicationFactory
public partial class Program { }
