using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI.Workflows;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Service that generates travel itineraries using the 4-agent AI workflow.
/// </summary>
public class ItineraryService(
	[FromKeyedServices("itinerary-workflow")] Workflow workflow,
	LanguagePreferenceService languagePreference)
{
	private static readonly JsonSerializerOptions s_jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter() },
	};

	/// <summary>
	/// Streams an itinerary for a specific landmark (legacy UI-driven flow).
	/// Constructs a natural language request from the parameters.
	/// </summary>
	public async IAsyncEnumerable<ItineraryStreamUpdate> StreamItineraryAsync(
		Landmark landmark,
		int dayCount,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// Build natural language request from UI parameters
		var language = languagePreference.SelectedLanguage;
		var userRequest = language.Equals("English", StringComparison.OrdinalIgnoreCase)
			? $"Create a {dayCount}-day itinerary for {landmark.Name}"
			: $"Create a {dayCount}-day itinerary for {landmark.Name} in {language}";

		await foreach (var update in StreamItineraryAsync(userRequest, cancellationToken))
		{
			yield return update;
		}
	}

	/// <summary>
	/// Streams an itinerary from a natural language request.
	/// Example: "Give me a 5-day Maui itinerary in French"
	/// </summary>
	public async IAsyncEnumerable<ItineraryStreamUpdate> StreamItineraryAsync(
		string input,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// Execute the 4-agent workflow with streaming
		await using var run = await InProcessExecution.StreamAsync(workflow, input);
		await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

		await foreach (var evt in run.WatchStreamAsync().WithCancellation(cancellationToken))
		{
			if (evt is WorkflowOutputEvent outputEvent && outputEvent.Data is string json)
			{
				// Parse the JSON output into an Itinerary
				Itinerary? itinerary = null;
				try
				{
					itinerary = JsonSerializer.Deserialize<Itinerary>(json, s_jsonOptions);
				}
				catch
				{
					// Skip if JSON is invalid
				}

				if (itinerary is not null)
				{
					yield return new ItineraryStreamUpdate
					{
						PartialItinerary = itinerary,
						IsTranslated = !languagePreference.SelectedLanguage.Equals("English", StringComparison.OrdinalIgnoreCase)
					};
				}
			}
		}
	}
}

/// <summary>
/// Update from the itinerary streaming process.
/// </summary>
public record ItineraryStreamUpdate
{
	public ToolLookup? ToolLookup { get; init; }
	public ToolLookup? ToolLookupResult { get; init; }
	public Itinerary? PartialItinerary { get; init; }
	public bool IsTranslated { get; init; }
}
