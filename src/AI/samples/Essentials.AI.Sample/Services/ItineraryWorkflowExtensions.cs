using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Extension methods to register the 4-agent itinerary workflow.
/// </summary>
public static class ItineraryWorkflowExtensions
{
	/// <summary>
	/// Registers the itinerary workflow with 4 specialized agents:
	/// 1. Travel Planner - parses natural language, extracts intent (no tools)
	/// 2. Researcher - finds best matching landmark (tool: getLandmarks)
	/// 3. Itinerary Planner - builds the itinerary (tool: findPointsOfInterest)
	/// 4. Translator - translates if non-English (conditional, no tools)
	/// </summary>
	public static IHostApplicationBuilder AddItineraryWorkflow(this IHostApplicationBuilder builder)
	{
		// Agent 1: Travel Planner - parses natural language, extracts intent
		builder.AddAIAgent(
			name: "travel-planner-agent",
			instructions: """
				You are a travel planning assistant. Parse the user's request to extract:
				1. landmarkName: The destination they want to visit
				2. dayCount: Number of days for the trip (default: 3)
				3. language: Language for the itinerary (default: English)
				
				Examples:
				- "5-day trip to Maui in French" → landmarkName: "Maui", dayCount: 5, language: "French"
				- "Visit the Great Wall" → landmarkName: "Great Wall of China", dayCount: 3, language: "English"
				- "Itinerary for Tokyo" → landmarkName: "Tokyo", dayCount: 3, language: "English"
				""");

		// Agent 2: Researcher - finds best matching landmark
		builder.AddAIAgent(
			name: "researcher-agent",
			instructions: """
				You are a travel researcher. Your job is to find the best matching destination.
				Use the getLandmarks tool to see what destinations are available.
				Match the user's requested destination to one of the available landmarks.
				Return the exact name of the best matching landmark.
				""");

		// Agent 3: Itinerary Planner - builds detailed itineraries
		builder.AddAIAgent(
			name: "itinerary-planner-agent",
			instructions: """
				You create detailed travel itineraries. For each day include:
				- An activity or attraction
				- A hotel recommendation  
				- A restaurant recommendation
				
				Use the findPointsOfInterest tool to discover places near the destination.
				Pass the landmarkName to findPointsOfInterest along with the category (Hotel, Restaurant, Museum, etc.).
				Give the itinerary a fun, creative title and engaging description.
				""");

		// Agent 4: Translator - translates content
		builder.AddAIAgent(
			name: "translator-agent",
			instructions: """
				You are a professional translator. Translate the provided JSON content to the target language.
				Preserve the JSON structure exactly - only translate the text values.
				Do not add explanations or commentary.
				""");

		// Register the workflow
		builder.AddWorkflow("itinerary-workflow", (sp, key) =>
		{
			var travelPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("travel-planner-agent");
			var researcherAgent = sp.GetRequiredKeyedService<AIAgent>("researcher-agent");
			var itineraryPlannerAgent = sp.GetRequiredKeyedService<AIAgent>("itinerary-planner-agent");
			var translatorAgent = sp.GetRequiredKeyedService<AIAgent>("translator-agent");
			var landmarkService = LandmarkDataService.Instance;

			// Create executors for each agent
			var travelPlannerExecutor = new TravelPlannerExecutor(travelPlannerAgent);
			var researcherExecutor = new ResearcherExecutor(researcherAgent, landmarkService);
			var itineraryPlannerExecutor = new ItineraryPlannerExecutor(itineraryPlannerAgent);
			var translatorExecutor = new TranslatorExecutor(translatorAgent);
			var outputExecutor = new OutputExecutor();

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
	/// Result from the Travel Planner Agent - parsed user intent.
	/// </summary>
	public record TravelPlanResult(string LandmarkName, int DayCount, string Language);

	/// <summary>
	/// Result from the Researcher Agent - the best matching landmark name (for JSON schema).
	/// </summary>
	internal record LandmarkMatchResult(
		[property: DisplayName("matchedLandmarkName")]
		[property: Description("The exact name of the best matching landmark from the available list.")]
		string MatchedLandmarkName);

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
	/// No tools - just NLP to extract landmarkName, dayCount, language.
	/// </summary>
	internal sealed class TravelPlannerExecutor(AIAgent agent) : Executor<string, TravelPlanResult>("TravelPlannerExecutor")
	{
		public override async ValueTask<TravelPlanResult> HandleAsync(
			string input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				ResponseFormat = ChatResponseFormat.ForJsonSchema<TravelPlanResult>(s_jsonOptions)
			});

			var response = await agent.RunAsync(input, options: runOptions, cancellationToken: cancellationToken);

			return JsonSerializer.Deserialize<TravelPlanResult>(response.Text, s_jsonOptions)!;
		}
	}

	/// <summary>
	/// Agent 2: Researcher - Uses AI to find the best matching landmark from available options.
	/// Tools: getLandmarks() - lists available landmarks
	/// The AI determines which landmark best matches the user's request.
	/// </summary>
	internal sealed class ResearcherExecutor(AIAgent agent, LandmarkDataService landmarkService) : Executor<TravelPlanResult, ResearchResult>("ResearcherExecutor")
	{
		public override async ValueTask<ResearchResult> HandleAsync(
			TravelPlanResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			// Create tool to list available landmarks
			var getLandmarksTool = AIFunctionFactory.Create(GetLandmarks);

			// Ask AI to find the best match from available landmarks
			var prompt = $"""
				The user is looking for: "{input.LandmarkName}"
				
				Use the getLandmarks tool to see what destinations are available.
				Then determine which landmark best matches the user's request.
				Return the exact name of the matching landmark.
				""";

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				Tools = [getLandmarksTool],
				ResponseFormat = ChatResponseFormat.ForJsonSchema<LandmarkMatchResult>(s_jsonOptions)
			});

			// Run agent - it will call getLandmarks and determine the best match
			var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			// Parse the AI's response to get the matched landmark name
			var matchResult = JsonSerializer.Deserialize<LandmarkMatchResult>(response.Text, s_jsonOptions);
			var matchedName = matchResult?.MatchedLandmarkName ?? input.LandmarkName;

			// Load the full landmark data using the AI's matched name
			var landmark = landmarkService.Landmarks
				.FirstOrDefault(l => l.Name.Equals(matchedName, StringComparison.OrdinalIgnoreCase));

			return new ResearchResult(landmark, input.DayCount, input.Language);
		}

		[DisplayName("getLandmarks")]
		[Description("Get a list of all available landmark/destination names that can be used for travel itineraries.")]
		private string[] GetLandmarks() =>
			landmarkService.GetLandmarkNames().ToArray();
	}

	/// <summary>
	/// Agent 3: Itinerary Planner - Builds the travel itinerary.
	/// Tools: findPointsOfInterest(landmarkName, category, query)
	/// </summary>
	internal sealed class ItineraryPlannerExecutor(AIAgent agent) : Executor<ResearchResult, ItineraryResult>("ItineraryPlannerExecutor")
	{
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
			if (input.Landmark is null)
			{
				return new ItineraryResult(JsonSerializer.Serialize(new { error = "Landmark not found" }), input.Language);
			}

			// Create tool from instance method
			var poiFunction = AIFunctionFactory.Create(FindPointsOfInterest);

			var prompt = BuildItineraryPrompt(input);

			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				Tools = [poiFunction],
				ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(s_jsonOptions)
			});

			var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			return new ItineraryResult(response.Text, input.Language);
		}

		[DisplayName("findPointsOfInterest")]
		[Description("Finds points of interest (hotels, restaurants, activities) near a landmark destination.")]
		private static string FindPointsOfInterest(
			[Description("The name of the landmark/destination to search near.")]
			string landmarkName,
			[Description("The category of place to find (Hotel, Restaurant, Cafe, Museum, etc.).")]
			PointOfInterestCategory category,
			[Description("Optional natural language query to refine the search.")]
			string? query = null)
		{
			var suggestions = GetSuggestions(category);
			return $"Near {landmarkName}, these {category} options are available: {string.Join(", ", suggestions)}";
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
				Always use the findPointsOfInterest tool with landmarkName="{input.Landmark.Name}" to find places.
				
				Categories available: {categories}
				
				Landmark description: {input.Landmark.Description}
				
				Example format (don't copy content):
				{example}
				""";
		}
	}

	/// <summary>
	/// Agent 4: Translator - Translates the itinerary to target language (conditional).
	/// No tools - just translation.
	/// </summary>
	internal sealed class TranslatorExecutor(AIAgent agent) : Executor<ItineraryResult, ItineraryResult>("TranslatorExecutor")
	{
		public override async ValueTask<ItineraryResult> HandleAsync(
			ItineraryResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			var runOptions = new ChatClientAgentRunOptions(new ChatOptions
			{
				ResponseFormat = ChatResponseFormat.ForJsonSchema<Itinerary>(s_jsonOptions)
			});

			var prompt = $"Translate to {input.TargetLanguage}. Preserve JSON structure exactly:\n\n{input.ItineraryJson}";

			var response = await agent.RunAsync(prompt, options: runOptions, cancellationToken: cancellationToken);

			return new ItineraryResult(response.Text, input.TargetLanguage);
		}
	}

	/// <summary>
	/// Final executor that outputs the itinerary JSON.
	/// </summary>
	internal sealed class OutputExecutor() : Executor<ItineraryResult>("OutputExecutor")
	{
		public override async ValueTask HandleAsync(
			ItineraryResult input,
			IWorkflowContext context,
			CancellationToken cancellationToken = default)
		{
			await context.YieldOutputAsync(input.ItineraryJson, cancellationToken);
		}
	}
}
