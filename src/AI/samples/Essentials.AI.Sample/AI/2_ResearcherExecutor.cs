using System.ComponentModel;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 2: Researcher - Uses RAG to find candidate destinations, then AI selects the best match.
/// Uses semantic search (embeddings) to pre-filter destinations, then LLM picks the best one.
/// </summary>
internal sealed class ResearcherExecutor(AIAgent agent, DataService dataService, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<TravelPlanResult, ResearchResult>("ResearcherExecutor")
{
	/// <summary>
	/// Maximum number of RAG candidates to return from semantic search.
	/// </summary>
	private const int MaxRagCandidates = 5;

	public const string Instructions = """
		You are a travel researcher.
		Your job is to select the best matching destination from a list of candidates.
		
		Rules:
		1. You will be given a list of candidate destinations that semantically match the user's request.
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

		// Step 1: Use RAG to find semantically similar destinations
		var candidates = await dataService.SearchLandmarksAsync(input.DestinationName, MaxRagCandidates);

		logger.LogDebug("[ResearcherExecutor] RAG returned {Count} candidates: {Names}",
			candidates.Count, string.Join(", ", candidates.Select(c => c.Name)));

		if (candidates.Count == 0)
		{
			logger.LogDebug("[ResearcherExecutor] No candidates found");
			await context.AddEventAsync(new ExecutorStatusEvent("No matching destinations found"));
			return new ResearchResult(null, input.DayCount, input.Language);
		}

		// If only one candidate, use it directly without LLM call
		if (candidates.Count == 1)
		{
			var singleMatch = candidates[0];
			logger.LogDebug("[ResearcherExecutor] Single candidate found: {Name}", singleMatch.Name);
			await context.AddEventAsync(new ExecutorStatusEvent($"Found destination: {singleMatch.Name}"));
			return new ResearchResult(singleMatch, input.DayCount, input.Language);
		}

		await context.AddEventAsync(new ExecutorStatusEvent($"Evaluating {candidates.Count} candidates..."));

		// Step 2: Ask LLM to pick the best match from RAG candidates
		var candidateDescriptions = string.Join("\n", candidates.Select(c =>
			$"- {c.Name}: {c.ShortDescription}"));

		var prompt = $"""
			The user wants to visit: "{input.DestinationName}"
			
			Here are the available destinations that might match:
			{candidateDescriptions}
			
			Which destination best matches what the user is looking for?
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

		logger.LogDebug("[ResearcherExecutor] AI selected '{MatchedName}' from candidates", matchedName);

		// Find the landmark from candidates (prefer exact match from candidates)
		var landmark = candidates.FirstOrDefault(l => l.Name.Equals(matchedName, StringComparison.OrdinalIgnoreCase))
			?? candidates[0]; // Fallback to top RAG result if LLM returned unexpected name

		var result = new ResearchResult(landmark, input.DayCount, input.Language);

		logger.LogDebug("[ResearcherExecutor] Completed - selected destination: {Name}", landmark.Name);
		logger.LogTrace("[ResearcherExecutor] Output: {@Result}", result);

		await context.AddEventAsync(new ExecutorStatusEvent($"Found destination: {landmark.Name}"));

		return result;
	}
}
