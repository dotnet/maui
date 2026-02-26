using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 2: Researcher - Uses TextSearchProvider (RAG) to automatically inject matching destinations
/// into the AI context before each invocation, then the AI selects the best match.
/// The TextSearchProvider is configured with BeforeAIInvoke mode via <see cref="CreateAgent"/>,
/// so candidate destinations are automatically searched and injected.
/// </summary>
internal sealed class ResearcherExecutor(AIAgent agent, ILogger logger)
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

	/// <summary>
	/// Creates an AIAgent with a TextSearchProvider that performs RAG via DataService.SearchLandmarksAsync.
	/// The provider runs in BeforeAIInvoke mode, automatically searching for matching landmarks
	/// and injecting them as context before each AI call.
	/// </summary>
	public static AIAgent CreateAgent(string name, IChatClient chatClient, DataService dataService, ILoggerFactory loggerFactory)
	{
		var ragLogger = loggerFactory.CreateLogger<TextSearchProvider>();

		var searchProvider = new TextSearchProvider(
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

		return chatClient.AsAIAgent(new ChatClientAgentOptions
		{
			Name = name,
			ChatOptions = new ChatOptions { Instructions = Instructions },
			AIContextProviders = [searchProvider],
		}, loggerFactory);
	}

	public override async ValueTask<ResearchResult> HandleAsync(
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
		var prompt = $"""
			The user wants to visit: "{input.DestinationName}"
			
			Which destination from the additional context best matches what the user is looking for?
			Include the destination's description from the context in your response.
			""";

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
