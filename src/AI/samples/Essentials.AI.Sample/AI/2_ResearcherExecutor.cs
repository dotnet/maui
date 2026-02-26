using System.Text.Json;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 2: Researcher - Uses TextSearchProvider (RAG) to automatically inject matching destinations
/// into the AI context before each invocation, then the AI selects the best match.
/// The TextSearchProvider is configured in ItineraryWorkflowExtensions with BeforeAIInvoke mode,
/// so candidate destinations are automatically searched and injected.
/// </summary>
internal sealed class ResearcherExecutor(AIAgent agent, DataService dataService, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<TravelPlanResult, ResearchResult>("ResearcherExecutor")
{
	public const string Instructions = """
		You are a travel researcher.
		Your job is to select the best matching destination from the additional context provided.
		
		Rules:
		1. You will be given additional context containing candidate destinations that match the user's request.
		2. Select the ONE destination that best matches what the user asked for.
		3. NEVER make up destinations - only choose from the provided candidates.
		4. If none of the candidates match well, pick the closest one.
		
		Return the exact name of the best matching destination from the candidates.
		""";

	public override async ValueTask<ResearchResult> HandleAsync(
		TravelPlanResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[ResearcherExecutor] Starting - finding best matching destination for '{DestinationName}'", input.DestinationName);
		logger.LogTrace("[ResearcherExecutor] Input: {@Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Searching destinations..."));

		// TextSearchProvider (configured in ItineraryWorkflowExtensions) automatically
		// searches DataService.SearchLandmarksAsync and injects results as context
		// before the AI call. We just need to ask the AI to pick the best match.
		var prompt = $"""
			The user wants to visit: "{input.DestinationName}"
			
			Which destination from the additional context best matches what the user is looking for?
			""";

		logger.LogTrace("[ResearcherExecutor] Prompt: {Prompt}", prompt);

		var runOptions = new ChatClientAgentRunOptions(new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<DestinationMatchResult>(jsonOptions)
		});

		var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

		logger.LogTrace("[ResearcherExecutor] Raw response: {Response}", response.Text);

		// Parse the AI's response to get the matched destination name
		var matchResult = JsonSerializer.Deserialize<DestinationMatchResult>(response.Text, jsonOptions);
		var matchedName = matchResult?.MatchedDestinationName ?? input.DestinationName;

		logger.LogDebug("[ResearcherExecutor] AI selected '{MatchedName}'", matchedName);

		// Resolve the matched name back to a Landmark object
		var landmarks = await dataService.SearchLandmarksAsync(matchedName, maxResults: 1);
		var landmark = landmarks.FirstOrDefault();

		if (landmark is null)
		{
			logger.LogDebug("[ResearcherExecutor] Could not resolve landmark for '{MatchedName}'", matchedName);
			await context.AddEventAsync(new ExecutorStatusEvent("No matching destinations found"));
			return new ResearchResult(null, input.DayCount, input.Language);
		}

		var result = new ResearchResult(landmark, input.DayCount, input.Language);

		logger.LogDebug("[ResearcherExecutor] Completed - selected destination: {Name}", landmark.Name);
		logger.LogTrace("[ResearcherExecutor] Output: {@Result}", result);

		await context.AddEventAsync(new ExecutorStatusEvent($"Found destination: {landmark.Name}"));

		return result;
	}
}
