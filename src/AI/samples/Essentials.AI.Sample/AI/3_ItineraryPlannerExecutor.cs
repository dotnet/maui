using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 3: Itinerary Planner - Builds the travel itinerary with streaming output.
/// Tools are used to assist in generating the itinerary.
/// Uses RunStreamingAsync to emit partial JSON as it's generated.
/// </summary>
internal sealed partial class ItineraryPlannerExecutor(AIAgent agent, ILogger logger)
	: Executor("ItineraryPlannerExecutor")
{
	[MessageHandler]
	private async ValueTask<ItineraryResult> HandleAsync(
		ResearchResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[ItineraryPlannerExecutor] Starting - building {Days}-day itinerary for '{Destination}'",
			input.DayCount, input.DestinationName ?? "unknown");
		logger.LogTrace("[ItineraryPlannerExecutor] Input: {@Input}", input);

		await context.AddEventAsync(new ExecutorStatusEvent("Building your itinerary..."), cancellationToken);

		if (input.DestinationName is null)
		{
			logger.LogDebug("[ItineraryPlannerExecutor] No destination found - returning error");
			await context.AddEventAsync(new ExecutorStatusEvent("Error: No destination found"), cancellationToken);
			return new ItineraryResult(System.Text.Json.JsonSerializer.Serialize(new { error = "Destination not found" }), input.Language);
		}

		var prompt = $"""
			Generate a {input.DayCount}-day itinerary to {input.DestinationName}.
			Destination description: {input.DestinationDescription}
			""";

		logger.LogTrace("[ItineraryPlannerExecutor] Prompt: {Prompt}", prompt);

		// Use streaming to emit partial JSON as it's generated
		// Tools and ResponseFormat are configured at agent level in ItineraryWorkflowExtensions
		var fullResponse = new StringBuilder();
		await foreach (var update in agent.RunStreamingAsync(prompt, cancellationToken: cancellationToken))
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

		await context.AddEventAsync(new ExecutorStatusEvent($"Created {input.DayCount}-day itinerary for {input.DestinationName}"), cancellationToken);

		return new ItineraryResult(responseText, input.Language);
	}
}
