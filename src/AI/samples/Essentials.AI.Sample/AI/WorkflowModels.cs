using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Result from the Travel Planner Agent - raw extraction of user intent.
/// Short JSON names (place/days/language) reduce misspelling by small language models.
/// </summary>
public record TravelPlanResult(
	[property: JsonPropertyName("place")]
	[property: Description("The destination name mentioned by the user.")]
	string DestinationName,
	[property: JsonPropertyName("days")]
	[property: Description("Number of days for the trip. Default is 3.")]
	int DayCount,
	[property: JsonPropertyName("language")]
	[property: Description("Output language for the itinerary. Default is English.")]
	string Language);

/// <summary>
/// Result from the Researcher Agent - the best matching destination (for JSON schema).
/// </summary>
internal record DestinationMatchResult(
	[property: JsonPropertyName("dest")]
	[property: Description("The exact name of the best matching destination from the available list.")]
	string MatchedDestinationName,
	[property: JsonPropertyName("desc")]
	[property: Description("A brief description of the matched destination, based on the information provided in the additional context.")]
	string MatchedDestinationDescription);

/// <summary>
/// Result from the Researcher Agent - includes destination name and description from RAG context.
/// </summary>
public record ResearchResult(
	string? DestinationName,
	string? DestinationDescription,
	int DayCount,
	string Language);

/// <summary>
/// Result from the Itinerary Planner Agent.
/// </summary>
public record ItineraryResult(
	string ItineraryJson,
	string TargetLanguage);
