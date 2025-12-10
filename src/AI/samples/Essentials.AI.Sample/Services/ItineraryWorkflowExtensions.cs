using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Extension methods to register the 4-agent itinerary workflow.
/// </summary>
public static class ItineraryWorkflowExtensions
{
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
			instructions: """
				You are a simple text parser. Extract ONLY these 3 values from the user's request:
				
				1. destinationName: The place/location name mentioned (extract it exactly as written)
				2. dayCount: The number of days mentioned (default: 3 if not specified)
				3. language: The language mentioned for the output (default: English if not specified)
				
				IMPORTANT: Do NOT interpret or expand the destination. Just extract the raw text.
				
				Examples:
				- "5-day trip to Maui in French" → destinationName: "Maui", dayCount: 5, language: "French"
				- "Visit the Great Wall" → destinationName: "Great Wall", dayCount: 3, language: "English"
				- "Itinerary for Tokyo" → destinationName: "Tokyo", dayCount: 3, language: "English"
				- "Give me a Maui itinerary" → destinationName: "Maui", dayCount: 3, language: "English"
				- "Plan a 7 day Japan trip in Spanish" → destinationName: "Japan", dayCount: 7, language: "Spanish"
				""",
				"local-model");

		// Agent 2: Researcher - finds best matching destination
		builder.AddAIAgent(
			name: "researcher-agent",
			instructions: """
				You are a travel researcher. Your job is to match the user's destination to an available one.
				Use the getDestinations tool to see what destinations are available in the database.
				Find the destination that best matches what the user requested.
				Return the exact name of the matching destination from the available list.
				""",
				"local-model");

		// Agent 3: Itinerary Planner - builds detailed itineraries
		builder.AddAIAgent(
			name: "itinerary-planner-agent",
			instructions: """
				You create detailed travel itineraries. For each day include:
				- An activity or attraction
				- A hotel recommendation  
				- A restaurant recommendation
				
				Use the findPointsOfInterest tool to discover places near the destination.
				Pass the destinationName to findPointsOfInterest along with the category (Hotel, Restaurant, Museum, etc.).
				Give the itinerary a fun, creative title and engaging description.
				""",
				"local-model");

		// Agent 4: Translator - translates content
		builder.AddAIAgent(
			name: "translator-agent",
			instructions: """
				You are a professional translator. Translate the provided JSON content to the target language.
				Preserve the JSON structure exactly - only translate the text values.
				Do not add explanations or commentary.
				""",
				"cloud-model");

		// Register the workflow
		builder.AddWorkflow("itinerary-workflow", (sp, key) =>
		{
			var travelPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("travel-planner-agent");
			var researcherAgent = sp.GetRequiredKeyedService<AIAgent>("researcher-agent");
			var itineraryPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("itinerary-planner-agent");
			var translatorAgent = sp.GetRequiredKeyedService<AIAgent>("translator-agent");
			var landmarkService = LandmarkDataService.Instance;
			var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("ItineraryWorkflow");

			// Create executors for each agent with logging
			var travelPlannerExecutor = new TravelPlannerExecutor(travelPlannerAgent, logger);
			var researcherExecutor = new ResearcherExecutor(researcherAgent, landmarkService, logger);
			var itineraryPlannerExecutor = new ItineraryPlannerExecutor(itineraryPlannerAgent, logger);
			var translatorExecutor = new TranslatorExecutor(translatorAgent, logger);
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

	/// <summary>
	/// Result from the Travel Planner Agent - raw extraction of user intent.
	/// These values should be extracted exactly as the user stated them, with no interpretation or expansion.
	/// </summary>
	public record TravelPlanResult(
		[property: DisplayName("destinationName")]
		[property: Description("The exact place/location name as written in the user's request. Extract the raw text only - do NOT interpret, expand, or look up actual landmarks. Example: 'Maui' not 'Maui, Hawaii' or 'Haleakala National Park'.")]
		string DestinationName,
		[property: DisplayName("dayCount")]
		[property: Description("The exact number of days mentioned by the user. Use 3 as default only if no number is specified.")]
		int DayCount,
		[property: DisplayName("language")]
		[property: Description("The exact output language mentioned by the user. Use 'English' as default only if no language is specified.")]
		string Language);

	/// <summary>
	/// Result from the Researcher Agent - the best matching destination name (for JSON schema).
	/// </summary>
	internal record DestinationMatchResult(
		[property: DisplayName("matchedDestinationName")]
		[property: Description("The exact name of the best matching destination from the available list.")]
		string MatchedDestinationName);

	/// <summary>
	/// Result from the Researcher Agent - includes full landmark details.
	/// </summary>
	public record ResearchResult(Landmark? Landmark, int DayCount, string Language);

	/// <summary>
	/// Result from the Itinerary Planner Agent.
	/// </summary>
	public record ItineraryResult(string ItineraryJson, string TargetLanguage);

	private static readonly JsonSerializerOptions s_jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() },
	};

	/// <summary>
	/// Agent 1: Travel Planner - Parses natural language to extract intent.
	/// No tools - just NLP to extract destinationName, dayCount, language.
	/// </summary>
	internal sealed class TravelPlannerExecutor(AIAgent agent, ILogger logger) : Executor<string, TravelPlanResult>("TravelPlannerExecutor")
	{
		public override async ValueTask<TravelPlanResult> HandleAsync(
			string input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			logger.LogDebug("[TravelPlannerExecutor] Starting - parsing user intent");
			logger.LogTrace("[TravelPlannerExecutor] Input: {Input}", input);

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				ResponseFormat = ChatResponseFormat.ForJsonSchema<TravelPlanResult>(s_jsonOptions)
			});

			var response = await agent.RunAsync(input, options: runOptions, cancellationToken: cancellationToken);
			
			logger.LogTrace("[TravelPlannerExecutor] Raw response: {Response}", response.Text);

			var result = JsonSerializer.Deserialize<TravelPlanResult>(response.Text, s_jsonOptions)!;
			
			logger.LogDebug("[TravelPlannerExecutor] Completed - extracted: destination={Destination}, days={Days}, language={Language}", 
				result.DestinationName, result.DayCount, result.Language);

			return result;
		}
	}

	/// <summary>
	/// Agent 2: Researcher - Uses AI to find the best matching destination from available options.
	/// Tools: getDestinations() - lists available destinations
	/// The AI determines which destination best matches the user's request.
	/// </summary>
	internal sealed class ResearcherExecutor(AIAgent agent, LandmarkDataService landmarkService, ILogger logger) : Executor<TravelPlanResult, ResearchResult>("ResearcherExecutor")
	{
		public override async ValueTask<ResearchResult> HandleAsync(
			TravelPlanResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			logger.LogDebug("[ResearcherExecutor] Starting - finding best matching destination for '{DestinationName}'", input.DestinationName);
			logger.LogTrace("[ResearcherExecutor] Input: {@Input}", input);

			// Ask AI to find the best match from available destinations
			var prompt = $"""
				The user is looking for: "{input.DestinationName}"
				
				Use the getDestinations tool to see what destinations are available in our database.
				Then determine which destination best matches the user's request.
				Return the exact name of the matching destination.
				""";

			logger.LogTrace("[ResearcherExecutor] Prompt: {Prompt}", prompt);

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				ToolMode = ChatToolMode.None,
				Tools = [AIFunctionFactory.Create(GetDestinations)],
				ResponseFormat = ChatResponseFormat.ForJsonSchema<DestinationMatchResult>(s_jsonOptions)
			});

			// Run agent - it will call getDestinations and determine the best match
			var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			logger.LogTrace("[ResearcherExecutor] Raw response: {Response}", response.Text);

			// Parse the AI's response to get the matched destination name
			var matchResult = JsonSerializer.Deserialize<DestinationMatchResult>(response.Text, s_jsonOptions);
			var matchedName = matchResult?.MatchedDestinationName ?? input.DestinationName;

			logger.LogDebug("[ResearcherExecutor] AI matched '{RequestedName}' to '{MatchedName}'", input.DestinationName, matchedName);

			// Load the full landmark data using the AI's matched name
			var landmark = landmarkService.Landmarks
				.FirstOrDefault(l => l.Name.Equals(matchedName, StringComparison.OrdinalIgnoreCase));

			var result = new ResearchResult(landmark, input.DayCount, input.Language);
			
			logger.LogDebug("[ResearcherExecutor] Completed - destination found: {Found}", landmark is not null);
			logger.LogTrace("[ResearcherExecutor] Output: {@Result}", result);

			return result;
		}

		[DisplayName("getDestinations")]
		[Description("Get a list of all available destination names that can be used for travel itineraries.")]
		private string[] GetDestinations()
		{
			var destinations = landmarkService.GetDestinationNames().ToArray();
			logger.LogTrace("[ResearcherExecutor] getDestinations tool called - returning {Count} destinations: {Destinations}", 
				destinations.Length, string.Join(", ", destinations));
			return destinations;
		}
	}

	/// <summary>
	/// Agent 3: Itinerary Planner - Builds the travel itinerary.
	/// Tools: findPointsOfInterest(destinationName, category, query)
	/// </summary>
	internal sealed class ItineraryPlannerExecutor : Executor<ResearchResult, ItineraryResult>
	{
		private readonly AIAgent _agent;
		private readonly ILogger _logger;

		public ItineraryPlannerExecutor(AIAgent agent, ILogger logger) : base("ItineraryPlannerExecutor")
		{
			_agent = agent;
			_logger = logger;
		}

		enum PointOfInterestCategory
		{
			Cafe,
			Campground,
			Hotel,
			Marina,
			Museum,
			NationalMonument,
			Restaurant,
		}

		public override async ValueTask<ItineraryResult> HandleAsync(
			ResearchResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			_logger.LogDebug("[ItineraryPlannerExecutor] Starting - building {Days}-day itinerary for '{Landmark}'", 
				input.DayCount, input.Landmark?.Name ?? "unknown");
			_logger.LogTrace("[ItineraryPlannerExecutor] Input: {@Input}", input);

			if (input.Landmark is null)
			{
				_logger.LogDebug("[ItineraryPlannerExecutor] No landmark found - returning error");
				return new ItineraryResult(JsonSerializer.Serialize(new { error = "Landmark not found" }), input.Language);
			}

			var prompt = BuildItineraryPrompt(input);
			_logger.LogTrace("[ItineraryPlannerExecutor] Prompt: {Prompt}", prompt);

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				Tools = [AIFunctionFactory.Create(FindPointsOfInterest)],
				ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(s_jsonOptions)
			});

			var response = await _agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			_logger.LogTrace("[ItineraryPlannerExecutor] Raw response: {Response}", response.Text);
			_logger.LogDebug("[ItineraryPlannerExecutor] Completed - itinerary generated, language: {Language}", input.Language);

			return new ItineraryResult(response.Text, input.Language);
		}

		[DisplayName("findPointsOfInterest")]
		[Description("Finds points of interest (hotels, restaurants, activities) near a destination.")]
		private string FindPointsOfInterest(
			[Description("The name of the destination to search near.")]
			string destinationName,
			[Description("The category of place to find (Hotel, Restaurant, Cafe, Museum, etc.).")]
			PointOfInterestCategory category,
			[Description("Optional natural language query to refine the search.")]
			string? query = null)
		{
			var suggestions = GetSuggestions(category);
			var result = $"Near {destinationName}, these {category} options are available: {string.Join(", ", suggestions)}";
			_logger.LogTrace("[ItineraryPlannerExecutor] findPointsOfInterest tool called - destination={Destination}, category={Category}, query={Query}, result={Result}",
				destinationName, category, query ?? "(none)", result);
			return result;
		}

		private static string[] GetSuggestions(PointOfInterestCategory category) =>
			category switch
			{
				PointOfInterestCategory.Cafe => ["Cafe 1", "Cafe 2", "Cafe 3"],
				PointOfInterestCategory.Campground => ["Campground 1", "Campground 2", "Campground 3"],
				PointOfInterestCategory.Hotel => ["Hotel 1", "Hotel 2", "Hotel 3"],
				PointOfInterestCategory.Marina => ["Marina 1", "Marina 2", "Marina 3"],
				PointOfInterestCategory.Museum => ["Museum 1", "Museum 2", "Museum 3"],
				PointOfInterestCategory.NationalMonument => ["The National Rock 1", "The National Rock 2", "The National Rock 3"],
				PointOfInterestCategory.Restaurant => ["Restaurant 1", "Restaurant 2", "Restaurant 3"],
				_ => []
			};

		private static string BuildItineraryPrompt(ResearchResult input)
		{
			var categories = string.Join(", ", Enum.GetNames<PointOfInterestCategory>());
			var example = JsonSerializer.Serialize(Itinerary.GetExampleTripToJapan(), s_jsonOptions);

			return $"""
				Generate a {input.DayCount}-day itinerary to {input.Landmark!.Name}.
				Give it a fun title and description.
				Each day needs an activity, hotel and restaurant.
				Always use the findPointsOfInterest tool with destinationName="{input.Landmark.Name}" to find places.
				
				Categories available: {categories}
				
				Destination description: {input.Landmark.Description}
				
				Example format (don't copy content):
				{example}
				""";
		}
	}

	/// <summary>
	/// Agent 4: Translator - Translates the itinerary to target language (conditional).
	/// No tools - just translation.
	/// </summary>
	internal sealed class TranslatorExecutor(AIAgent agent, ILogger logger) : Executor<ItineraryResult, ItineraryResult>("TranslatorExecutor")
	{
		public override async ValueTask<ItineraryResult> HandleAsync(
			ItineraryResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			logger.LogDebug("[TranslatorExecutor] Starting - translating to '{Language}'", input.TargetLanguage);
			logger.LogTrace("[TranslatorExecutor] Input JSON: {Json}", input.ItineraryJson);

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(s_jsonOptions)
			});

			var prompt = $"Translate to {input.TargetLanguage}. Preserve JSON structure exactly:\n\n{input.ItineraryJson}";
			logger.LogTrace("[TranslatorExecutor] Prompt: {Prompt}", prompt);

			var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			logger.LogTrace("[TranslatorExecutor] Raw response: {Response}", response.Text);
			logger.LogDebug("[TranslatorExecutor] Completed - translation to '{Language}' finished", input.TargetLanguage);

			return new ItineraryResult(response.Text, input.TargetLanguage);
		}
	}

	/// <summary>
	/// Final executor that outputs the itinerary JSON.
	/// </summary>
	internal sealed class OutputExecutor(ILogger logger) : Executor<ItineraryResult>("OutputExecutor")
	{
		public override async ValueTask HandleAsync(
			ItineraryResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			logger.LogDebug("[OutputExecutor] Starting - outputting final itinerary (language: {Language})", input.TargetLanguage);
			logger.LogTrace("[OutputExecutor] Final JSON: {Json}", input.ItineraryJson);

			await context.YieldOutputAsync(input.ItineraryJson, cancellationToken);

			logger.LogDebug("[OutputExecutor] Completed - workflow finished");
		}
	}
}
