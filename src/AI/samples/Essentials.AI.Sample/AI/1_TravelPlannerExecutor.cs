using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Agent 1: Travel Planner - Parses natural language to extract intent.
/// No tools - just NLP to extract destinationName, dayCount, language.
/// </summary>
internal sealed class TravelPlannerExecutor(AIAgent agent, ILogger logger)
	: ChatProtocolExecutor("TravelPlannerExecutor", new ChatProtocolExecutorOptions { AutoSendTurnToken = false })
{
	/// <summary>
	/// Declares TravelPlanResult as a sent message type so the edge router can map it to downstream executors.
	/// Without this, ChatProtocolExecutor only declares List&lt;ChatMessage&gt; and TurnToken, causing
	/// TravelPlanResult to be silently dropped with DroppedTypeMismatch.
	/// </summary>
	protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
		=> base.ConfigureProtocol(protocolBuilder).SendsMessage<TravelPlanResult>();

	protected override async ValueTask TakeTurnAsync(
		List<ChatMessage> messages,
		IWorkflowContext context,
		bool? emitEvents = null,
		CancellationToken cancellationToken = default)
	{
		logger.LogDebug("[TravelPlannerExecutor] Starting - parsing user intent");

		await context.AddEventAsync(new ExecutorStatusEvent("Analyzing your request..."), cancellationToken);

		var response = await agent.RunAsync<TravelPlanResult>(messages, cancellationToken: cancellationToken);

		logger.LogTrace("[TravelPlannerExecutor] Raw response: {Response}", response.Text);

		var result = response.Result;

		logger.LogDebug("[TravelPlannerExecutor] Completed - extracted: destination={Destination}, days={Days}, language={Language}",
			result.DestinationName, result.DayCount, result.Language);

		var summary = result.Language != "English"
			? $"Planning {result.DayCount}-day trip to {result.DestinationName} in {result.Language}"
			: $"Planning {result.DayCount}-day trip to {result.DestinationName}";
		await context.AddEventAsync(new ExecutorStatusEvent(summary), cancellationToken);

		await context.SendMessageAsync(result, cancellationToken);
	}
}
