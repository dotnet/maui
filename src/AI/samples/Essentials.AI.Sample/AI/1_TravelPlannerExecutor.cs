using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 1: Travel Planner - Parses natural language to extract intent.
/// No tools - just NLP to extract destinationName, dayCount, language.
/// </summary>
internal sealed class TravelPlannerExecutor(AIAgent agent, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<string, TravelPlanResult>("TravelPlannerExecutor")
{
	public const string Instructions = """
		You are a simple text parser. 
		
		Extract ONLY these 3 values from the user's request:
		1. destinationName: The place/location name mentioned (extract it exactly as written)
		2. dayCount: The number of days mentioned (default: 3 if not specified)
		3. language: The language mentioned for the output (default: English if not specified)
		
		Rules:
		1. ALWAYS extract the raw values.
		2. NEVER make up values or interpret the user's intent.
		
		Examples:
		- "5-day trip to Maui in French" → destinationName: "Maui", dayCount: 5, language: "French"
		- "Visit the Great Wall" → destinationName: "Great Wall", dayCount: 3, language: "English"
		- "Itinerary for Tokyo" → destinationName: "Tokyo", dayCount: 3, language: "English"
		- "Give me a Maui itinerary" → destinationName: "Maui", dayCount: 3, language: "English"
		- "Plan a 7 day Japan trip in Spanish" → destinationName: "Japan", dayCount: 7, language: "Spanish"
		""";

	public override async ValueTask<TravelPlanResult> HandleAsync(
		string input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[TravelPlannerExecutor] Starting - parsing user intent");
		logger.LogTrace("[TravelPlannerExecutor] Input: {Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Analyzing your request..."));

		var runOptions = new ChatClientAgentRunOptions(new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<TravelPlanResult>(jsonOptions)
		});

		var response = await agent.RunAsync(input, options: runOptions, cancellationToken: cancellationToken);

		logger.LogTrace("[TravelPlannerExecutor] Raw response: {Response}", response.Text);

		var result = JsonSerializer.Deserialize<TravelPlanResult>(response.Text, jsonOptions)!;

		logger.LogDebug("[TravelPlannerExecutor] Completed - extracted: destination={Destination}, days={Days}, language={Language}",
			result.DestinationName, result.DayCount, result.Language);

		var summary = result.Language != "English"
			? $"Planning {result.DayCount}-day trip to {result.DestinationName} in {result.Language}"
			: $"Planning {result.DayCount}-day trip to {result.DestinationName}";
		await context.AddEventAsync(new ExecutorStatusEvent(summary));

		return result;
	}
}
