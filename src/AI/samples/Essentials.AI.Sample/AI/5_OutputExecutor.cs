using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Final executor that marks the workflow as complete.
/// The itinerary JSON has already been streamed by ItineraryPlannerExecutor or TranslatorExecutor.
/// </summary>
internal sealed partial class OutputExecutor(ILogger logger)
	: Executor("OutputExecutor")
{
	[MessageHandler]
	private async ValueTask HandleAsync(
		ItineraryResult input,
		IWorkflowContext context,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[OutputExecutor] Starting - finalizing itinerary (language: {Language})", input.TargetLanguage);
		logger.LogTrace("[OutputExecutor] Final JSON: {Json}", input.ItineraryJson);

		// Don't re-emit the JSON - it was already streamed by ItineraryPlannerExecutor or TranslatorExecutor
		await context.AddEventAsync(new ExecutorStatusEvent("Your itinerary is ready!"), cancellationToken);

		logger.LogDebug("[OutputExecutor] Completed - workflow finished");
	}
}
