using System.ComponentModel;
using System.Text.Json;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 2: Researcher - Uses AI to find the best matching destination from available options.
/// Tools: getDestinations() - lists available destinations
/// The AI determines which destination best matches the user's request.
/// </summary>
internal sealed class ResearcherExecutor(AIAgent agent, LandmarkDataService landmarkService, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<TravelPlanResult, ResearchResult>("ResearcherExecutor")
{
	private IWorkflowContext? _context;

	public const string Instructions = $"""
		You are a travel researcher.
		Your job is to match the user's destination to an available one.
		
		Rules:
		1. ALWAYS use the `{GetDestinationsToolName}` tool to see what destinations are available in the database.
		2. NEVER use your own knowledge or make up destinations.
		3. ALWAYS use the destinations returned by the tool.
		
		Find the destination that best matches what the user requested.
		Return the exact name of the matching destination from the available list.
		""";

	public const string GetDestinationsToolName = "getDestinations";

	public override async ValueTask<ResearchResult> HandleAsync(
		TravelPlanResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		_context = context;

		logger.LogDebug("[ResearcherExecutor] Starting - finding best matching destination for '{DestinationName}'", input.DestinationName);
		logger.LogTrace("[ResearcherExecutor] Input: {@Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Searching destinations..."));

		// Ask AI to find the best match from available destinations
		var prompt = $"""
			Find destinations for a trip to "{input.DestinationName}"
			""";

		logger.LogTrace("[ResearcherExecutor] Prompt: {Prompt}", prompt);

		var runOptions = new ChatClientAgentRunOptions(new ChatOptions
		{
			Tools = [AIFunctionFactory.Create(GetDestinationsAsync, name: GetDestinationsToolName)],
			ResponseFormat = ChatResponseFormat.ForJsonSchema<DestinationMatchResult>(jsonOptions)
		});

		// Run agent - it will call getDestinations and determine the best match
		var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

		logger.LogTrace("[ResearcherExecutor] Raw response: {Response}", response.Text);

		// Parse the AI's response to get the matched destination name
		var matchResult = JsonSerializer.Deserialize<DestinationMatchResult>(response.Text, jsonOptions);
		var matchedName = matchResult?.MatchedDestinationName ?? input.DestinationName;

		logger.LogDebug("[ResearcherExecutor] AI matched '{RequestedName}' to '{MatchedName}'", input.DestinationName, matchedName);

		// Load the full landmark data using the AI's matched name
		var landmark = landmarkService.Landmarks
			.FirstOrDefault(l => l.Name.Equals(matchedName, StringComparison.OrdinalIgnoreCase));

		var result = new ResearchResult(landmark, input.DayCount, input.Language);

		logger.LogDebug("[ResearcherExecutor] Completed - destination found: {Found}", landmark is not null);
		logger.LogTrace("[ResearcherExecutor] Output: {@Result}", result);

		var statusMsg = landmark is not null ? $"Found destination: {landmark.Name}" : "No matching destination found";
		await context.AddEventAsync(new ExecutorStatusEvent(statusMsg));

		return result;
	}

	[Description("Get a list of all available destination names that can be used for travel itineraries.")]
	private async Task<string[]> GetDestinationsAsync()
	{
		if (_context is not null)
		{
			await _context.AddEventAsync(new ExecutorStatusEvent("Fetching available destinations..."));
		}

		var destinations = landmarkService.GetDestinationNames().ToArray();
		logger.LogTrace("[ResearcherExecutor] getDestinations tool called - returning {Count} destinations: {Destinations}",
			destinations.Length, string.Join(", ", destinations));

		if (_context is not null)
		{
			await _context.AddEventAsync(new ExecutorStatusEvent($"Found {destinations.Length} destinations"));
		}

		return destinations;
	}
}
