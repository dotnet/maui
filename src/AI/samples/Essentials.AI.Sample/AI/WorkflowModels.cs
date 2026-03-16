using System.ComponentModel;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Result from the Travel Planner Agent - raw extraction of user intent.
/// These values should be extracted exactly as the user stated them, with no interpretation or expansion.
/// </summary>
public record TravelPlanResult(
	[property: DisplayName("destinationName")]
	[property: Description("The exact place/location name as written in the user's request. Extract the raw text only - do NOT interpret, expand, or look up actual landmarks. Example: 'Maui' not 'Maui, Hawaii' or 'Haleakala National Park'.")]
	string DestinationName,
	[property: DisplayName("dayCount")]
	[property: Description("The exact number of days mentioned by the user. Use 3 as default only if no number is specified.")]
	int DayCount,
	[property: DisplayName("language")]
	[property: Description("The exact output language mentioned by the user. Use 'English' as default only if no language is specified.")]
	string Language);

/// <summary>
/// Result from the Researcher Agent - the best matching destination (for JSON schema).
/// </summary>
internal record DestinationMatchResult(
	[property: DisplayName("matchedDestinationName")]
	[property: Description("The exact name of the best matching destination from the available list.")]
	string MatchedDestinationName,
	[property: DisplayName("matchedDestinationDescription")]
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
