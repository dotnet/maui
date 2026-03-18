using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 4: Translator - Translates the itinerary to target language (conditional) with streaming.
/// No tools - just translation.
/// Uses RunStreamingAsync to emit partial translated JSON.
/// </summary>
internal sealed partial class TranslatorExecutor(AIAgent agent, ILogger logger)
	: Executor("TranslatorExecutor")
{
	[MessageHandler]
	private async ValueTask<ItineraryResult> HandleAsync(
		ItineraryResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[TranslatorExecutor] Starting - translating to '{Language}'", input.TargetLanguage);
		logger.LogTrace("[TranslatorExecutor] Input JSON: {Json}", input.ItineraryJson);

		await context.AddEventAsync(new ExecutorStatusEvent($"Translating to {input.TargetLanguage}..."), cancellationToken);

		var prompt = $"""
			Translate to {input.TargetLanguage}:
			
			{input.ItineraryJson}
			""";

		logger.LogTrace("[TranslatorExecutor] Prompt: {Prompt}", prompt);

		// Use streaming to emit partial JSON as it's generated
		// ResponseFormat is set at agent creation time in ItineraryWorkflowExtensions
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

		logger.LogTrace("[TranslatorExecutor] Raw response: {Response}", responseText);
		logger.LogDebug("[TranslatorExecutor] Completed - translation to '{Language}' finished", input.TargetLanguage);

		await context.AddEventAsync(new ExecutorStatusEvent($"Translated to {input.TargetLanguage}"), cancellationToken);

		return new ItineraryResult(responseText, input.TargetLanguage);
	}
}
