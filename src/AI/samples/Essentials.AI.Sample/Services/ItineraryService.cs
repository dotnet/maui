using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services;

public record ItineraryStreamUpdate
{
    public string? ToolLookup { get; init; }
    public Itinerary? PartialItinerary { get; init; }
}

public class ItineraryService
{
    // Service for AI-powered itinerary generation
    // In the Apple version, this uses FoundationModels/SystemLanguageModel with streaming
    // We'll implement this with Microsoft.Extensions.AI when available

    public bool IsAvailable()
    {
        // TODO: Check if AI services are available
        // For now, always return true for stub implementation
        return true;
    }

    public string GetAvailabilityMessage()
    {
        if (IsAvailable())
            return string.Empty;

        // TODO: Return appropriate message based on AI service status
        return "Trip Planner is unavailable. Please ensure AI services are enabled.";
    }

    public async IAsyncEnumerable<ItineraryStreamUpdate> StreamItineraryAsync(
        Landmark landmark, 
        int dayCount,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // TODO: Implement with Microsoft.Extensions.AI streaming
        // For now, simulate streaming with progressive updates

        // Simulate tool lookups (finding points of interest)
        var toolCategories = new[] { "restaurants", "hotels", "museums", "cafes" };
        foreach (var category in toolCategories)
        {
            await Task.Delay(500, cancellationToken);
            yield return new ItineraryStreamUpdate
            {
                ToolLookup = category
            };
        }

        // Start building itinerary
        await Task.Delay(800, cancellationToken);

        // Simulate progressive streaming of itinerary data
        var days = new List<DayPlan>();

        for (int i = 0; i < dayCount; i++)
        {
            await Task.Delay(600, cancellationToken);
            
            days.Add(new DayPlan
            {
                Title = $"Day {i + 1}: {GetDayTitle(i)}",
                Subtitle = GetDaySubtitle(i),
                Destination = landmark.Name,
                WeatherForecast = GetWeatherForecast(),
                Activities = GenerateActivities(i)
            });

            // Yield progressive update
            yield return new ItineraryStreamUpdate
            {
                PartialItinerary = new Itinerary
                {
                    Title = $"Amazing {landmark.Name} Adventure",
                    DestinationName = landmark.Name,
                    Description = $"A wonderful {dayCount}-day trip to {landmark.Name}",
                    Rationale = GenerateRationale(landmark),
                    Days = new List<DayPlan>(days)
                }
            };
        }
    }

    private static string GetDayTitle(int dayIndex) => dayIndex switch
    {
        0 => "Arrival and Exploration",
        1 => "Main Attractions",
        2 => "Final Adventures",
        _ => "Continued Exploration"
    };

    private static string GetDaySubtitle(int dayIndex) => dayIndex switch
    {
        0 => "Getting to know the area",
        1 => "Experience the highlights",
        2 => "Last day adventures",
        _ => "More to discover"
    };

    private static string GetWeatherForecast()
    {
        var temps = new[] { "72°F", "68°F", "75°F", "70°F" };
        return $"☁️ {temps[Random.Shared.Next(temps.Length)]}";
    }

    private static string GenerateRationale(Landmark landmark)
    {
        return $"This itinerary is carefully crafted to showcase the best of {landmark.Name}, " +
               "balancing iconic attractions with authentic local experiences. " +
               "Each day includes a mix of activities, dining, and accommodation options.";
    }

    private static List<Activity> GenerateActivities(int dayIndex)
    {
        return dayIndex switch
        {
            0 => new List<Activity>
            {
                new Activity { Type = ActivityKind.HotelAndLodging, Title = "Hotel 1", Description = "Check into your comfortable accommodation" },
                new Activity { Type = ActivityKind.Sightseeing, Title = "Welcome Tour", Description = "Take an orientation tour of the area" },
                new Activity { Type = ActivityKind.FoodAndDining, Title = "Restaurant 1", Description = "Enjoy dinner at a local favorite" }
            },
            1 => new List<Activity>
            {
                new Activity { Type = ActivityKind.Sightseeing, Title = "Museum 1", Description = "Explore the history and culture" },
                new Activity { Type = ActivityKind.FoodAndDining, Title = "Cafe 1", Description = "Lunch at a charming cafe" },
                new Activity { Type = ActivityKind.Shopping, Title = "Local Market", Description = "Browse artisan crafts and souvenirs" }
            },
            _ => new List<Activity>
            {
                new Activity { Type = ActivityKind.FoodAndDining, Title = "Restaurant 2", Description = "Breakfast with a view" },
                new Activity { Type = ActivityKind.Sightseeing, Title = "Final Sights", Description = "Last photo opportunities" },
                new Activity { Type = ActivityKind.HotelAndLodging, Title = "Hotel 1", Description = "Check-out and departure" }
            }
        };
    }
}

