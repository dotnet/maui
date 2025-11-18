namespace Maui.Controls.Sample.Models;

public record Itinerary
{
    public required string Title { get; init; }
    public required string DestinationName { get; init; }
    public required string Description { get; init; }
    public required string Rationale { get; init; }
    public required List<DayPlan> Days { get; init; }
}

public record DayPlan
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string Destination { get; init; }
    public required List<Activity> Activities { get; init; }
    public string? WeatherForecast { get; set; }
}

public record Activity
{
    public required ActivityKind Type { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}

public enum ActivityKind
{
    Sightseeing,
    FoodAndDining,
    Shopping,
    HotelAndLodging
}

public static class ActivityKindExtensions
{
    public static string GetSymbolName(this ActivityKind kind) => kind switch
    {
        ActivityKind.Sightseeing => "binoculars",
        ActivityKind.FoodAndDining => "fork.knife",
        ActivityKind.Shopping => "cart",
        ActivityKind.HotelAndLodging => "bed.double",
        _ => "circle"
    };
}
