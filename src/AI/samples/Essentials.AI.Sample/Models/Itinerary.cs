using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Maui.Controls.Sample.Models;

[DisplayName("Itinerary")]
[Description("A travel itinerary with days and activities.")]
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

[DisplayName("DayPlan")]
public record DayPlan
{
	[Description("A unique and exciting title for this day plan.")]
	public required string Title { get; init; }

	public required string Subtitle { get; init; }

	public required string Destination { get; init; }

	[Length(3, 3)]
	public required List<Activity> Activities { get; init; }
}

[DisplayName("Activity")]
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
