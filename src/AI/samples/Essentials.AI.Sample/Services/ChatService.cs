using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class ChatService
{
	const string SystemPrompt = """
		You are a helpful travel assistant for the .NET MAUI Trip Planner app. You have access to 21 world landmarks across 7 continents and can help users:
		- Search and discover destinations
		- Find nearby hotels, restaurants, cafes, and museums
		- Check weather forecasts
		- Generate social media hashtags for trips
		- Change the AI response language
		- Start planning a trip by navigating to the trip planner

		When users ask about destinations, search first to provide relevant results. When they want to plan a trip, use plan_trip to navigate them to the planning page.
		Be concise and helpful. Use the tools available to provide accurate information.
		""";

	readonly IChatClient _toolClient;
	readonly DataService _dataService;
	readonly WeatherService _weatherService;
	readonly TaggingService _taggingService;
	readonly LanguagePreferenceService _languageService;
	readonly IList<AITool> _tools;

	public event Action<Landmark>? NavigateToTripRequested;

	public ChatService(
		IChatClient chatClient,
		DataService dataService,
		WeatherService weatherService,
		TaggingService taggingService,
		LanguagePreferenceService languageService)
	{
		_dataService = dataService;
		_weatherService = weatherService;
		_taggingService = taggingService;
		_languageService = languageService;

		_tools =
		[
			AIFunctionFactory.Create(SearchLandmarksAsync),
			AIFunctionFactory.Create(ListLandmarksByContinentAsync),
			AIFunctionFactory.Create(GetLandmarkDetailsAsync),
			AIFunctionFactory.Create(SearchPointsOfInterestAsync),
			AIFunctionFactory.Create(GetWeatherAsync),
			AIFunctionFactory.Create(GenerateTagsAsync),
			AIFunctionFactory.Create(SetLanguage),
			AIFunctionFactory.Create(PlanTripAsync),
		];

		// Don't use FunctionInvokingChatClient here — Apple Intelligence handles
		// tool calling natively at the Swift layer. The tools are passed via ChatOptions
		// and invoked directly by FoundationModels through AIFunctionToolAdapter.
		_toolClient = chatClient;
	}

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IList<ChatMessage> messages, CancellationToken cancellationToken = default)
	{
		// Prepend system prompt if not already present
		if (messages.Count == 0 || messages[0].Role != ChatRole.System)
		{
			messages.Insert(0, new ChatMessage(ChatRole.System, SystemPrompt));
		}

		var options = new ChatOptions { Tools = _tools };

		return _toolClient.GetStreamingResponseAsync(messages, options, cancellationToken);
	}

	[Description("Search for travel destinations by a natural language query. Uses semantic search to find the most relevant landmarks.")]
	async Task<string> SearchLandmarksAsync(
		[Description("A natural language search query, e.g. 'beaches in Asia' or 'ancient ruins'")] string query,
		[Description("Maximum number of results to return (1-10)")] int maxResults = 5)
	{
		var results = await _dataService.SearchLandmarksAsync(query, Math.Clamp(maxResults, 1, 10));

		if (results.Count == 0)
			return "No landmarks found matching your query.";

		var sb = new StringBuilder();
		foreach (var landmark in results)
		{
			sb.AppendLine($"• **{landmark.Name}** ({landmark.Continent})");
			sb.AppendLine($"  {landmark.ShortDescription}");
		}
		return sb.ToString();
	}

	[Description("List all available landmarks in a specific continent.")]
	async Task<string> ListLandmarksByContinentAsync(
		[Description("The continent name, e.g. 'Europe', 'Asia', 'South America', 'Africa', 'North America', 'Australia/Oceania', 'Antarctica'")] string continent)
	{
		var groups = await _dataService.GetLandmarksByContinentAsync();

		var match = groups.Keys.FirstOrDefault(k =>
			k.Contains(continent, StringComparison.OrdinalIgnoreCase));

		if (match is null || !groups.TryGetValue(match, out var landmarks))
			return $"No landmarks found for continent '{continent}'. Available: {string.Join(", ", groups.Keys)}";

		var sb = new StringBuilder();
		sb.AppendLine($"Landmarks in {match}:");
		foreach (var landmark in landmarks)
		{
			sb.AppendLine($"• **{landmark.Name}** — {landmark.ShortDescription}");
		}
		return sb.ToString();
	}

	[Description("Get detailed information about a specific landmark including its full description and location coordinates.")]
	async Task<string> GetLandmarkDetailsAsync(
		[Description("The name of the landmark to look up")] string landmarkName)
	{
		var landmark = await FindLandmarkByNameAsync(landmarkName);

		if (landmark is null)
			return $"Landmark '{landmarkName}' not found. Try searching with search_landmarks first.";

		return $"""
			**{landmark.Name}**
			Continent: {landmark.Continent}
			Coordinates: {landmark.Latitude:F4}, {landmark.Longitude:F4}

			{landmark.Description}
			""";
	}

	[Description("Find points of interest near a destination. Categories: Hotel, Restaurant, Cafe, Museum, Campground, Marina, NationalMonument.")]
	async Task<string> SearchPointsOfInterestAsync(
		[Description("The category of place to find")] PointOfInterestCategory category,
		[Description("A natural language query to refine the search, e.g. 'family friendly' or 'luxury'")] string query)
	{
		var results = await _dataService.SearchPointsOfInterestAsync(category, query, 5);

		if (results.Count == 0)
			return $"No {category} found matching your query.";

		var sb = new StringBuilder();
		sb.AppendLine($"Found {results.Count} {category} options:");
		foreach (var poi in results)
		{
			sb.AppendLine($"• **{poi.Name}** — {poi.Description}");
		}
		return sb.ToString();
	}

	[Description("Get the weather forecast for a landmark on a specific date. Returns temperature and conditions.")]
	async Task<string> GetWeatherAsync(
		[Description("The name of the landmark to check weather for")] string landmarkName,
		[Description("The date to check weather for in yyyy-MM-dd format. Use today's date if not specified.")] string date)
	{
		var landmark = await FindLandmarkByNameAsync(landmarkName);

		if (landmark is null)
			return $"Landmark '{landmarkName}' not found.";

		if (!DateOnly.TryParse(date, out var dateOnly))
			dateOnly = DateOnly.FromDateTime(DateTime.Now);

		var weather = await _weatherService.GetWeatherForecastAsync(
			landmark.Latitude, landmark.Longitude, dateOnly);

		return $"Weather at {landmark.Name} on {dateOnly:yyyy-MM-dd}: {weather}";
	}

	[Description("Generate social media hashtags for a trip description or destination.")]
	async Task<string> GenerateTagsAsync(
		[Description("The text to generate hashtags for, e.g. a trip description or destination name")] string text)
	{
		try
		{
			var tags = await _taggingService.GenerateTagsAsync(text);
			return $"Suggested hashtags: {string.Join(" ", tags.Select(t => $"#{t}"))}";
		}
		catch
		{
			return "Unable to generate tags at this time.";
		}
	}

	[Description("Change the language for AI-generated responses. Supported: English, French, Spanish, German, Chinese, Japanese, Korean, Arabic, Indonesian, Italian, Portuguese.")]
	string SetLanguage(
		[Description("The language name to switch to, e.g. 'Spanish', 'French', 'Japanese'")] string language)
	{
		var match = _languageService.SupportedLanguages.Keys.FirstOrDefault(k =>
			k.Equals(language, StringComparison.OrdinalIgnoreCase));

		if (match is null)
			return $"Language '{language}' is not supported. Available: {string.Join(", ", _languageService.SupportedLanguages.Keys)}";

		_languageService.SelectedLanguage = match;
		return $"Language changed to {match}. AI-generated itineraries will now be in {match}.";
	}

	[Description("Navigate the user to the trip planning page to generate a detailed multi-day itinerary for a landmark. Use this when the user wants to plan or start a trip.")]
	async Task<string> PlanTripAsync(
		[Description("The name of the landmark to plan a trip to")] string landmarkName)
	{
		var landmark = await FindLandmarkByNameAsync(landmarkName);

		if (landmark is null)
			return $"Landmark '{landmarkName}' not found. Try searching with search_landmarks first.";

		NavigateToTripRequested?.Invoke(landmark);
		return $"Navigating to trip planner for {landmark.Name}! A multi-day itinerary will be generated for you.";
	}

	async Task<Landmark?> FindLandmarkByNameAsync(string name)
	{
		var landmarks = await _dataService.GetLandmarksAsync();
		return landmarks.FirstOrDefault(l =>
			l.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
	}
}
