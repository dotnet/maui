using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
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
	[Length(3, 3)]
	public required List<DayPlan> Days { get; init; }

	public static JsonElement ToJsonSchema(IEnumerable<string> landmarks)
	{
		var schema = AIJsonUtilities.CreateJsonSchema(
			typeof(Itinerary),
			inferenceOptions: new AIJsonSchemaCreateOptions
			{
				TransformSchemaNode = TransformSchemaNode,
				TransformOptions = new AIJsonSchemaTransformOptions
				{
					DisallowAdditionalProperties = true
				}
			});

		return schema;

		JsonNode TransformSchemaNode(AIJsonSchemaCreateContext context, JsonNode schema)
		{
			if (schema is not JsonObject obj)
			{
				return schema;
			}

			// Add enum constraint for destinationName
			if (context.PropertyInfo?.Name.Equals(nameof(DestinationName), StringComparison.OrdinalIgnoreCase) == true)
			{
				var enumArray = new JsonArray();
				foreach (var landmark in landmarks)
				{
					enumArray.Add(JsonValue.Create(landmark));
				}
				obj["enum"] = enumArray;
			}

			if (obj.TryGetPropertyValue("type", out var typeNode) && typeNode?.GetValue<string>() == "object")
			{
				// Add title for object type definitions only
				if (context.TypeInfo is not null && !obj.ContainsKey("title"))
				{
					obj["title"] = context.TypeInfo.Type.Name;
				}
			}

			// Add x-order for property ordering
			if (obj.TryGetPropertyValue("properties", out var props) && props is JsonObject propsObj)
			{
				var order = new JsonArray();
				foreach (var prop in propsObj)
				{
					order.Add(JsonValue.Create(prop.Key));
				}
				obj["x-order"] = order;
			}

			return schema;
		}
	}

	public static Itinerary GetExampleTripToJapan() =>
		new()
		{
			Title = "Onsen Trip to Japan",
			DestinationName = "Mt. Fuji",
			Description = "Sushi, hot springs, and ryokan with a toddler!",
			Rationale =
				"""
				You are traveling with a child, so climbing Mt. Fuji is probably not an option,
				but there is lots to do around Kawaguchiko Lake, including Fujikyu.
				I recommend staying in a ryokan because you love hotsprings.
				""",
			Days = [
				new DayPlan
				{
					Title = "Sushi and Shopping Near Kawaguchiko",
					Subtitle = "Spend your final day enjoying sushi and souvenir shopping.",
					Destination = "Kawaguchiko Lake",
					Activities = [
						new Activity
						{
							Type = ActivityKind.FoodAndDining,
							Title = "The Restaurant serving Sushi",
							Description = "Visit an authentic sushi restaurant for lunch."
						},
						new Activity
						{
							Type = ActivityKind.Shopping,
							Title = "The Plaza",
							Description = "Enjoy souvenir shopping at various shops."
						},
						new Activity
						{
							Type = ActivityKind.Sightseeing,
							Title = "The Beautiful Cherry Blossom Park",
							Description = "Admire the beautiful cherry blossom trees in the park."
						},
						new Activity
						{
							Type = ActivityKind.HotelAndLodging,
							Title = "The Hotel",
							Description = "Spend one final evening in the hotspring before heading home."
						}]
				}]
		};
}

public record DayPlan
{
	[Description("A unique and exciting title for this day plan.")]
	public required string Title { get; init; }

	public required string Subtitle { get; init; }

	public required string Destination { get; init; }

	[Length(3, 3)]
	public required List<Activity> Activities { get; init; }
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
