using System.Text;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 4: Translator - Translates the itinerary to target language (conditional) with streaming.
/// No tools - just translation. Uses RunStreamingAsync to emit partial translated JSON.
/// </summary>
internal sealed class TranslatorExecutor(AIAgent agent, JsonSerializerOptions jsonOptions, ILogger logger)
	: Executor<ItineraryResult, ItineraryResult>("TranslatorExecutor")
{
	public const string Instructions = """
		You are a professional translator.
		Translate the provided JSON content to the target language.

		Rules:
		1. ALWAYS preserve the JSON format exactly.
		2. ONLY translate the text values within the JSON.
		3. NEVER add explanations or commentary.
		""";

	public override async ValueTask<ItineraryResult> HandleAsync(
		ItineraryResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[TranslatorExecutor] Starting - translating to '{Language}'", input.TargetLanguage);
		logger.LogTrace("[TranslatorExecutor] Input JSON: {Json}", input.ItineraryJson);

		await context.AddEventAsync(new ExecutorStatusEvent($"Translating to {input.TargetLanguage}..."));

		var runOptions = new ChatClientAgentRunOptions(new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(jsonOptions)
		});

		var prompt = $"""
			Translate to {input.TargetLanguage}:
			
			{input.ItineraryJson}
			""";

		logger.LogTrace("[TranslatorExecutor] Prompt: {Prompt}", prompt);

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

		logger.LogTrace("[TranslatorExecutor] Raw response: {Response}", responseText);
		logger.LogDebug("[TranslatorExecutor] Completed - translation to '{Language}' finished", input.TargetLanguage);

		await context.AddEventAsync(new ExecutorStatusEvent($"Translated to {input.TargetLanguage}"));

		return new ItineraryResult(responseText, input.TargetLanguage);
	}
}
