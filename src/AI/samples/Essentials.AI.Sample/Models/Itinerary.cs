using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Models;

public record Itinerary
{
    [Description("An exciting name for the trip.")]
    public required string Title { get; init; }
    
    public required string DestinationName { get; init; }
    
    public required string Description { get; init; }
    
    [Description("An explanation of how the itinerary meets the person's special requests.")]
    public required string Rationale { get; init; }
    
    [Description("A list of day-by-day plans.")]
    public required List<DayPlan> Days { get; init; }

    public static AIJsonSchemaCreateOptions GetSchemaOptions()
    {
        return new AIJsonSchemaCreateOptions
        {
            TransformSchemaNode = (context, schema) =>
            {
                // Add count constraints to match Swift's @Guide(.count(3))
                if (context.PropertyInfo != null)
                {
                    var propertyName = context.PropertyInfo.Name;
                    
                    // Days property should have exactly 3 items
                    if (propertyName == "Days" && schema is System.Text.Json.Nodes.JsonObject daysObj)
                    {
                        daysObj["minItems"] = 3;
                        daysObj["maxItems"] = 3;
                    }
                    
                    // Activities property should have exactly 3 items
                    if (propertyName == "Activities" && schema is System.Text.Json.Nodes.JsonObject activitiesObj)
                    {
                        activitiesObj["minItems"] = 3;
                        activitiesObj["maxItems"] = 3;
                    }
                }
                
                return schema;
            }
        };
    }

    public static JsonElement GetJsonSchema()
    {
        return AIJsonUtilities.CreateJsonSchema(typeof(Itinerary), inferenceOptions: GetSchemaOptions());
    }

    public static Itinerary GetExample()
    {
        return new Itinerary
        {
            Title = "Onsen Trip to Japan",
            DestinationName = "Mt. Fuji",
            Description = "Sushi, hot springs, and ryokan with a toddler!",
            Rationale = "You are traveling with a child, so climbing Mt. Fuji is probably not an option, but there is lots to do around Kawaguchiko Lake, including Fujikyu. I recommend staying in a ryokan because you love hotsprings.",
            Days = new List<DayPlan>
            {
                new DayPlan
                {
                    Title = "Sushi and Shopping Near Kawaguchiko",
                    Subtitle = "Spend your final day enjoying sushi and souvenir shopping.",
                    Destination = "Kawaguchiko Lake",
                    Activities = new List<Activity>
                    {
                        new Activity { Type = ActivityKind.FoodAndDining, Title = "The Restaurant serving Sushi", Description = "Visit an authentic sushi restaurant for lunch." },
                        new Activity { Type = ActivityKind.Shopping, Title = "The Plaza", Description = "Enjoy souvenir shopping at various shops." },
                        new Activity { Type = ActivityKind.Sightseeing, Title = "The Beautiful Cherry Blossom Park", Description = "Admire the beautiful cherry blossom trees in the park." },
                        new Activity { Type = ActivityKind.HotelAndLodging, Title = "The Hotel", Description = "Spend one final evening in the hotspring before heading home." }
                    }
                }
            }
        };
    }
}

public record DayPlan
{
    [Description("A unique and exciting title for this day plan.")]
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
