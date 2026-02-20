using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 3: Itinerary Planner - Builds the travel itinerary with streaming output.
/// Tools: findPointsOfInterest(destinationName, category, query)
/// Uses RunStreamingAsync to emit partial JSON as it's generated.
/// </summary>
internal sealed class ItineraryPlannerExecutor(AIAgent agent, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<ResearchResult, ItineraryResult>("ItineraryPlannerExecutor")
{
	private IWorkflowContext? _context;

	public const string Instructions = $"""
		You create detailed travel itineraries.
		
		For each day include these places:
		1. An activity or attraction
		2. A hotel recommendation  
		3. A restaurant recommendation
		
		Rules:
		1. ALWAYS use the `{FindPointsOfInterestToolName}` tool to discover real places near the destination.
		2. NEVER make up places or use your own knowledge.
		3. ONLY use places returned by the `{FindPointsOfInterestToolName}` tool.
		4. PREFER the places returned by the `{FindPointsOfInterestToolName}` tool instead of the destination description.
		
		Give the itinerary a fun, creative title and engaging description.

		Include a rationale explaining why you chose these activities for the traveler.
		""";

	public const string FindPointsOfInterestToolName = "findPointsOfInterest";

	public override async ValueTask<ItineraryResult> HandleAsync(
		ResearchResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		_context = context;

		logger.LogDebug("[ItineraryPlannerExecutor] Starting - building {Days}-day itinerary for '{Landmark}'",
			input.DayCount, input.Landmark?.Name ?? "unknown");
		logger.LogTrace("[ItineraryPlannerExecutor] Input: {@Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Building your itinerary..."));

		if (input.Landmark is null)
		{
			logger.LogDebug("[ItineraryPlannerExecutor] No landmark found - returning error");
			await context.AddEventAsync(new ExecutorStatusEvent("Error: No destination found"));
			return new ItineraryResult(JsonSerializer.Serialize(new { error = "Landmark not found" }), input.Language);
		}

		var prompt = $"""
			Generate a {input.DayCount}-day itinerary to {input.Landmark.Name}.
			Destination description: {input.Landmark.Description}
			""";

		logger.LogTrace("[ItineraryPlannerExecutor] Prompt: {Prompt}", prompt);

		var runOptions = new ChatClientAgentRunOptions(new ChatOptions
		{
			Tools = [AIFunctionFactory.Create(FindPointsOfInterestAsync, name: FindPointsOfInterestToolName)],
			ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(jsonOptions)
		});

		// Use streaming to emit partial JSON as it's generated
		var fullResponse = new StringBuilder();
		await foreach (var update in agent.RunStreamingAsync(prompt, options: runOptions, cancellationToken: cancellationToken))
		{
			foreach (var content in update.Contents)
			{
				if (content is not TextContent textContent)
					continue;

				fullResponse.Append(textContent.Text);

				await context.AddEventAsync(new ItineraryTextChunkEvent(Id, textContent.Text), cancellationToken);
			}
		}
		var responseText = fullResponse.ToString();

		logger.LogTrace("[ItineraryPlannerExecutor] Raw response: {Response}", responseText);
		logger.LogDebug("[ItineraryPlannerExecutor] Completed - itinerary generated, language: {Language}", input.Language);

		await context.AddEventAsync(new ExecutorStatusEvent($"Created {input.DayCount}-day itinerary for {input.Landmark.Name}"));

		return new ItineraryResult(responseText, input.Language);
	}

	[Description("Finds points of interest (hotels, restaurants, activities) near a destination.")]
	private async Task<string> FindPointsOfInterestAsync(
		[Description("The name of the destination to search near.")]
		string destinationName,
		[Description("The category of place to find (Hotel, Restaurant, Cafe, Museum, etc.).")]
		PointOfInterestCategory category,
		[Description("A natural language query to refine the search.")]
		string additionalSearchQuery)
	{
		if (_context is not null)
		{
			await _context.AddEventAsync(new ExecutorStatusEvent($"Finding {category}s near {destinationName}..."));
		}

		var suggestions = GetSuggestions(category);
		var result = $"""
			These {category} options are available near {destinationName}:

			- {string.Join(Environment.NewLine + "- ", suggestions)}
			""";

		logger.LogTrace("[ItineraryPlannerExecutor] findPointsOfInterest tool called - destination={Destination}, category={Category}, query={Query}, result={Result}",
			destinationName, category, additionalSearchQuery ?? "(none)", result);

		if (_context is not null)
		{
			await _context.AddEventAsync(new ExecutorStatusEvent($"Found {suggestions.Length} {category} options"));
		}

		return result;
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
