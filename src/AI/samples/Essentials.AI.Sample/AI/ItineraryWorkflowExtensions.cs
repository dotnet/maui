using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Extension methods to register the 4-agent itinerary workflow.
/// </summary>
public static class ItineraryWorkflowExtensions
{
	internal static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() },
	};

	/// <summary>
	/// Registers the itinerary workflow with 4 specialized agents:
	/// 1. Travel Planner - parses natural language, extracts intent (no tools)
	/// 2. Researcher - finds best matching destination (tool: getDestinations)
	/// 3. Itinerary Planner - builds the itinerary (tool: findPointsOfInterest)
	/// 4. Translator - translates if non-English (conditional, no tools)
	/// </summary>
	public static IHostApplicationBuilder AddItineraryWorkflow(this IHostApplicationBuilder builder)
	{
		// Agent 1: Travel Planner - parses natural language, extracts intent
		builder.AddAIAgent(
			name: "travel-planner-agent",
			instructions: TravelPlannerExecutor.Instructions,
			chatClientServiceKey: "local-model");

		// Agent 2: Researcher - finds best matching destination
		builder.AddAIAgent(
			name: "researcher-agent",
			instructions: ResearcherExecutor.Instructions,
			chatClientServiceKey: "local-model");

		// Agent 3: Itinerary Planner - builds detailed itineraries
		builder.AddAIAgent(
			name: "itinerary-planner-agent",
			instructions: ItineraryPlannerExecutor.Instructions,
			chatClientServiceKey: "local-model");

		// Agent 4: Translator - translates content
		builder.AddAIAgent(
			name: "translator-agent",
			instructions: TranslatorExecutor.Instructions,
			chatClientServiceKey: "cloud-model");

		// Register the workflow
		builder.AddWorkflow("itinerary-workflow", (sp, key) =>
		{
			var travelPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("travel-planner-agent");
			var researcherAgent = sp.GetRequiredKeyedService<AIAgent>("researcher-agent");
			var itineraryPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("itinerary-planner-agent");
			var translatorAgent = sp.GetRequiredKeyedService<AIAgent>("translator-agent");
			var landmarkService = sp.GetRequiredService<LandmarkDataService>();
			var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ItineraryWorkflow");

			// Create executors for each agent with logging
			var travelPlannerExecutor = new TravelPlannerExecutor(travelPlannerAgent, JsonOptions, logger);
			var researcherExecutor = new ResearcherExecutor(researcherAgent, landmarkService, JsonOptions, logger);
			var itineraryPlannerExecutor = new ItineraryPlannerExecutor(itineraryPlannerAgent, JsonOptions, logger);
			var translatorExecutor = new TranslatorExecutor(translatorAgent, JsonOptions, logger);
			var outputExecutor = new OutputExecutor(logger);

			// Build the 4-agent workflow with conditional translation:
			// Travel Planner → Researcher → Itinerary Planner → (conditional) Translator → Output
			var workflow = new WorkflowBuilder(travelPlannerExecutor)
				.WithName(key)
				.AddEdge(travelPlannerExecutor, researcherExecutor)
				.AddEdge(researcherExecutor, itineraryPlannerExecutor)
				// English path: skip translation
				.AddEdge<ItineraryResult>(itineraryPlannerExecutor, outputExecutor, condition: IsEnglish)
				// Non-English path: translate first
				.AddEdge<ItineraryResult>(itineraryPlannerExecutor, translatorExecutor, condition: NeedsTranslation)
				.AddEdge(translatorExecutor, outputExecutor)
				.WithOutputFrom(outputExecutor)
				.Build();

			return workflow;
		});

		return builder;
	}

	private static bool IsEnglish(ItineraryResult? result) =>
		result is not null && string.Equals(result.TargetLanguage, "English", StringComparison.OrdinalIgnoreCase);

	private static bool NeedsTranslation(ItineraryResult? result) =>
		result is not null && !string.Equals(result.TargetLanguage, "English", StringComparison.OrdinalIgnoreCase);
}
