using System.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Static tool functions and content providers used by the itinerary workflow.
/// Defined here so they can be registered centrally in <see cref="ItineraryWorkflowExtensions"/>.
/// </summary>
internal static class ItineraryWorkflowTools
{
	public const string FindPointsOfInterestToolName = "findPointsOfInterest";

	/// <summary>
	/// Creates a <see cref="TextSearchProvider"/> that performs RAG via <see cref="DataService.SearchLandmarksAsync"/>.
	/// The provider runs in BeforeAIInvoke mode, automatically searching for matching landmarks
	/// and injecting them as context before each AI call.
	/// </summary>
	public static TextSearchProvider CreateLandmarkSearchProvider(DataService dataService, ILoggerFactory loggerFactory)
	{
		var ragLogger = loggerFactory.CreateLogger<TextSearchProvider>();

		return new TextSearchProvider(
			async (query, ct) =>
			{
				ragLogger.LogDebug("[RAG] Searching landmarks for query: '{Query}'", query);
				var results = await dataService.SearchLandmarksAsync(query, maxResults: 5);
				ragLogger.LogDebug("[RAG] Found {Count} landmarks: {Names}",
					results.Count, string.Join(", ", results.Select(r => r.Name)));
				return results.Select(r => new TextSearchProvider.TextSearchResult
				{
					Text = $"{r.Name}: {r.ShortDescription}",
					SourceName = r.Name,
				});
			},
			new TextSearchProviderOptions
			{
				SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
			},
			loggerFactory);
	}

	[Description("Finds points of interest (hotels, restaurants, activities) near a destination.")]
	public static Task<string> FindPointsOfInterestAsync(
		[Description("The name of the destination to search near.")]
		string destinationName,
		[Description("The category of place to find (Hotel, Restaurant, Cafe, Museum, etc.).")]
		PointOfInterestCategory category,
		[Description("A natural language query to refine the search.")]
		string additionalSearchQuery,
		IServiceProvider services)
	{
		var logger = services.GetService<ILoggerFactory>()?.CreateLogger("ItineraryWorkflowTools");

		var suggestions = GetSuggestions(category);
		var result = $"""
			These {category} options are available near {destinationName}:

			- {string.Join(Environment.NewLine + "- ", suggestions)}
			""";

		logger?.LogTrace("[ItineraryWorkflowTools] findPointsOfInterest - destination={Destination}, category={Category}, query={Query}, results={Count}",
			destinationName, category, additionalSearchQuery ?? "(none)", suggestions.Length);

		return Task.FromResult(result);
	}

	private static string[] GetSuggestions(PointOfInterestCategory category) =>
		category switch
		{
			PointOfInterestCategory.Cafe => ["Cafe 1", "Cafe 2", "Cafe 3"],
			PointOfInterestCategory.Campground => ["Campground 1", "Campground 2", "Campground 3"],
			PointOfInterestCategory.Hotel => ["Hotel 1", "Hotel 2", "Hotel 3"],
			PointOfInterestCategory.Marina => ["Marina 1", "Marina 2", "Marina 3"],
			PointOfInterestCategory.Museum => ["Museum 1", "Museum 2", "Museum 3"],
			PointOfInterestCategory.NationalMonument => ["The National Rock 1", "The National Rock 2", "The National Rock 3"],
			PointOfInterestCategory.Restaurant => ["Restaurant 1", "Restaurant 2", "Restaurant 3"],
			_ => []
		};
}
