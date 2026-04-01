using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 2: Researcher - Uses TextSearchProvider (RAG) to automatically inject matching destinations
/// into the AI context before each invocation, then the AI selects the best match.
/// The TextSearchProvider is configured in with BeforeAIInvoke mode, so candidate destinations are
/// automatically searched and injected.
/// </summary>
internal sealed partial class ResearcherExecutor(AIAgent agent, ILogger logger)
	: Executor("ResearcherExecutor")
{
	[MessageHandler]
	private async ValueTask<ResearchResult> HandleAsync(
		TravelPlanResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[ResearcherExecutor] Starting - finding best matching destination for '{DestinationName}'", input.DestinationName);
		logger.LogTrace("[ResearcherExecutor] Input: {@Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Searching destinations..."), cancellationToken);

		// TextSearchProvider (configured via CreateAgent) automatically searches
		// DataService.SearchLandmarksAsync and injects results as context before
		// the AI call. We just need to ask the AI to pick the best match.
		var prompt = input.DestinationName;

		logger.LogTrace("[ResearcherExecutor] Prompt: {Prompt}", prompt);

		var response = await agent.RunAsync<DestinationMatchResult>(prompt, cancellationToken: cancellationToken);

		logger.LogTrace("[ResearcherExecutor] Raw response: {Response}", response.Text);

		// Parse the AI's response — both name and description come from RAG context
		var matchResult = response.Result;

		logger.LogDebug("[ResearcherExecutor] AI selected '{MatchedName}'", matchResult.MatchedDestinationName);

		var result = new ResearchResult(
			matchResult.MatchedDestinationName,
			matchResult.MatchedDestinationDescription,
			input.DayCount,
			input.Language);

		logger.LogDebug("[ResearcherExecutor] Completed - selected destination: {Name}", matchResult.MatchedDestinationName);
		logger.LogTrace("[ResearcherExecutor] Output: {@Result}", result);

		await context.AddEventAsync(new ExecutorStatusEvent($"Found destination: {matchResult.MatchedDestinationName}"), cancellationToken);

		return result;
	}
}
