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
/// All agent configuration (instructions, tools, response formats, content providers) is
/// defined here. Executors contain only execution logic (streaming, status events, prompt assembly).
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
		// Tool: findPointsOfInterest - used by Agent 3
		var findPoiTool = AIFunctionFactory.Create(
			ItineraryWorkflowTools.FindPointsOfInterestAsync,
			name: ItineraryWorkflowTools.FindPointsOfInterestToolName);

		// Agent 1: Travel Planner - parses natural language, extracts intent
		builder.AddAIAgent(
			name: "travel-planner-agent",
			instructions: """
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
				""",
			chatClientServiceKey: "local-model");

		// Agent 2: Researcher - finds best matching destination using RAG via TextSearchProvider
		builder.AddAIAgent("researcher-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("local-model");
			var dataService = sp.GetRequiredService<DataService>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

			var searchProvider = ItineraryWorkflowTools.CreateLandmarkSearchProvider(dataService, loggerFactory);

			return chatClient.AsAIAgent(
				new ChatClientAgentOptions
				{
					Name = name,
					ChatOptions = new ChatOptions
					{
						Instructions = """
							You are a travel researcher.
							Your job is to select the best matching destination from the additional context provided.
							
							Rules:
							1. You will be given additional context containing candidate destinations that match the user's request.
							2. Select the ONE destination that best matches what the user asked for.
							3. NEVER make up destinations - only choose from the provided candidates.
							4. If none of the candidates match well, pick the closest one.
							5. Include the destination's description from the context in your response.
							
							Return the exact name of the best matching destination from the candidates.
							"""
					},
					AIContextProviders = [searchProvider],
				},
				loggerFactory);
		});

		// Agent 3: Itinerary Planner - builds detailed itineraries with tool calling
		builder.AddAIAgent("itinerary-planner-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("local-model");
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return chatClient.AsAIAgent(
				new ChatClientAgentOptions
				{
					Name = name,
					ChatOptions = new ChatOptions
					{
						Instructions = $"""
							You create detailed travel itineraries.
							
							For each day include these places:
							1. An activity or attraction
							2. A hotel recommendation  
							3. A restaurant recommendation
							
							Rules:
							1. ALWAYS use the `{ItineraryWorkflowTools.FindPointsOfInterestToolName}` tool to discover real places near the destination.
							2. NEVER make up places or use your own knowledge.
							3. ONLY use places returned by the `{ItineraryWorkflowTools.FindPointsOfInterestToolName}` tool.
							4. PREFER the places returned by the `{ItineraryWorkflowTools.FindPointsOfInterestToolName}` tool instead of the destination description.
							
							Give the itinerary a fun, creative title and engaging description.

							Include a rationale explaining why you chose these activities for the traveler.
							""",
						ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(JsonOptions),
						Tools = [findPoiTool],
					},
				},
				loggerFactory,
				services: sp);
		});

		// Agent 4: Translator - translates content with streaming
		builder.AddAIAgent("translator-agent", (sp, name) =>
		{
			var chatClient = sp.GetRequiredKeyedService<IChatClient>("cloud-model");
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return chatClient.AsAIAgent(
				new ChatClientAgentOptions
				{
					Name = name,
					ChatOptions = new ChatOptions
					{
						Instructions = """
							You are a professional translator.
							Translate the provided JSON content to the target language.

							Rules:
							1. ALWAYS preserve the JSON format exactly.
							2. ONLY translate the text values within the JSON.
							3. NEVER add explanations or commentary.
							""",
						ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(JsonOptions),
					},
				},
				loggerFactory);
		});

		// Register the workflow
		var workflow = builder.AddWorkflow("itinerary-workflow", (sp, key) =>
		{
			var travelPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("travel-planner-agent");
			var researcherAgent = sp.GetRequiredKeyedService<AIAgent>("researcher-agent");
			var itineraryPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("itinerary-planner-agent");
			var translatorAgent = sp.GetRequiredKeyedService<AIAgent>("translator-agent");
			var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ItineraryWorkflow");

			// Create executors — thin wrappers with just execution logic
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
					.AddCase<ItineraryResult>(r => r is not null && !string.Equals(r.TargetLanguage, "English", StringComparison.OrdinalIgnoreCase), translatorExecutor)
					.WithDefault(outputExecutor))
				.AddEdge(translatorExecutor, outputExecutor)
				.WithOutputFrom(outputExecutor)
				.Build();

			return workflow;
		});

		// Register the workflow as an AI agent for easy invocation
		workflow.AddAsAIAgent("itinerary-workflow-agent");

		return builder;
	}
}
