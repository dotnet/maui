using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.AI;
using Maui.Controls.Sample.Models;
using Microsoft.Agents.AI;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Service that generates travel itineraries using the 4-agent AI workflow.
/// Uses the workflow-as-agent pattern for a cleaner, more unified API.
/// </summary>
public class ItineraryService(
	[FromKeyedServices("itinerary-workflow-agent")] AIAgent workflowAgent,
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
		var deserializer = new StreamingJsonDeserializer<Itinerary>(s_jsonOptions);
		JsonMerger? merger = null;
		var lastExecutorId = string.Empty;

		// Stream responses from the workflow agent
		await foreach (var update in workflowAgent.RunStreamingAsync(input, cancellationToken: cancellationToken))
		{
			// Check RawRepresentation for our custom workflow events
			if (update.RawRepresentation is ExecutorStatusEvent statusEvent)
			{
				yield return new ItineraryStreamUpdate
				{
					StatusMessage = statusEvent.StatusMessage
				};
			}
			else if (update.RawRepresentation is ItineraryTextChunkEvent textChunk)
			{
				// Initialize last executor ID
				if (string.IsNullOrEmpty(lastExecutorId))
				{
					lastExecutorId = textChunk.ExecutorId;
				}

				// Detect executor switch (e.g., from ItineraryPlanner to Translator)
				if (lastExecutorId != textChunk.ExecutorId)
				{
					// Save the complete reconstructed JSON from the previous executor as the base for merging
					merger = new JsonMerger(deserializer.ReconstructedJsonUtf8.ToArray());
					lastExecutorId = textChunk.ExecutorId;
					deserializer.Reset();
				}

				var partialItinerary = deserializer.ProcessChunk(textChunk.TextChunk);
				if (partialItinerary is not null)
				{
					// If we have a merger (translation phase), merge with base itinerary
					if (merger is not null)
					{
						merger.MergeOverlayUtf8(deserializer.ReconstructedJsonMemory.Span);
						var mergedItinerary = merger.Deserialize<Itinerary>(s_jsonOptions);
						if (mergedItinerary is not null)
						{
							partialItinerary = mergedItinerary;
						}
					}

					yield return new ItineraryStreamUpdate
					{
						PartialItinerary = partialItinerary
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
	/// <summary>
	/// Status message to display in the fading status trail.
	/// </summary>
	public string? StatusMessage { get; init; }

	/// <summary>
	/// The partial or complete itinerary.
	/// </summary>
	public Itinerary? PartialItinerary { get; init; }
}
