using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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
	/// 1. Travel Planner - parses natural language, extracts intent (no tools, structured output)
	/// 2. Researcher - uses RAG (semantic search) to find candidates, AI picks the best match
	/// 3. Itinerary Planner - builds the itinerary with streaming (tool: findPointsOfInterest)
	/// 4. Translator - translates if non-English with streaming (conditional, no tools)
	/// </summary>
	public static IHostApplicationBuilder AddItineraryWorkflow(this IHostApplicationBuilder builder)
	{
		// Agent 1: Travel Planner - parses natural language, extracts intent
		builder.AddAIAgent(
			name: "travel-planner-agent",
			instructions: TravelPlannerExecutor.Instructions,
			chatClientServiceKey: "local-model");

		// Agent 2: Researcher - finds best matching destination using RAG via TextSearchProvider
		builder.AddAIAgent("researcher-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("local-model");
			var dataService = sp.GetRequiredService<DataService>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return ResearcherExecutor.CreateAgent(name, chatClient, dataService, loggerFactory);
		});

		// Agent 3: Itinerary Planner - builds detailed itineraries (ResponseFormat set at agent level)
		builder.AddAIAgent("itinerary-planner-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("local-model");
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return chatClient.AsAIAgent(new ChatClientAgentOptions
			{
				Name = name,
				ChatOptions = new ChatOptions
				{
					Instructions = ItineraryPlannerExecutor.Instructions,
					ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(JsonOptions),
				},
			}, loggerFactory);
		});

		// Agent 4: Translator - translates content (ResponseFormat set at agent level)
		builder.AddAIAgent("translator-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("cloud-model");
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return chatClient.AsAIAgent(new ChatClientAgentOptions
			{
				Name = name,
				ChatOptions = new ChatOptions
				{
					Instructions = TranslatorExecutor.Instructions,
					ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(JsonOptions),
				},
			}, loggerFactory);
		});

		// Register the workflow
		var workflow = builder.AddWorkflow("itinerary-workflow", (sp, key) =>
		{
			var travelPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("travel-planner-agent");
			var researcherAgent = sp.GetRequiredKeyedService<AIAgent>("researcher-agent");
			var itineraryPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("itinerary-planner-agent");
			var translatorAgent = sp.GetRequiredKeyedService<AIAgent>("translator-agent");
			var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ItineraryWorkflow");

			// Create executors for each agent with logging
			var travelPlannerExecutor = new TravelPlannerExecutor(travelPlannerAgent, logger);
			var researcherExecutor = new ResearcherExecutor(researcherAgent, logger);
			var itineraryPlannerExecutor = new ItineraryPlannerExecutor(itineraryPlannerAgent, logger);
			var translatorExecutor = new TranslatorExecutor(translatorAgent, logger);
			var outputExecutor = new OutputExecutor(logger);

			// Build the 4-agent workflow with conditional translation:
			// Travel Planner → Researcher → Itinerary Planner → (conditional) Translator → Output
			var workflow = new WorkflowBuilder(travelPlannerExecutor)
				.WithName(key)
				.AddEdge(travelPlannerExecutor, researcherExecutor)
				.AddEdge(researcherExecutor, itineraryPlannerExecutor)
				.AddSwitch(itineraryPlannerExecutor, switch_ => switch_
					.AddCase<ItineraryResult>(IsEnglish, outputExecutor)
					.AddCase<ItineraryResult>(NeedsTranslation, translatorExecutor))
				.AddEdge(translatorExecutor, outputExecutor)
				.WithOutputFrom(outputExecutor)
				.Build();

			return workflow;
		});

		// Register the workflow as an AI agent for easy invocation
		workflow.AddAsAIAgent("itinerary-workflow-agent");

		return builder;
	}

	private static bool IsEnglish(ItineraryResult? result) =>
		result is not null && string.Equals(result.TargetLanguage, "English", StringComparison.OrdinalIgnoreCase);

	private static bool NeedsTranslation(ItineraryResult? result) =>
		result is not null && !string.Equals(result.TargetLanguage, "English", StringComparison.OrdinalIgnoreCase);
}
