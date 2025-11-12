using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services.Tools;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class ItineraryService(IChatClient chatClient, LandmarkDataService landmarkService)
{
	public record ItineraryStreamUpdate(
		ToolLookup? ToolLookup = null,
		ToolLookup? ToolLookupResult = null,
		Itinerary? PartialItinerary = null);

	public async IAsyncEnumerable<ItineraryStreamUpdate> StreamItineraryAsync(
		Landmark landmark,
		int dayCount,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var findPointsOfInterestTool = new FindPointsOfInterestTool(landmark);
		var findPointsOfInterestFunction = AIFunctionFactory.Create(findPointsOfInterestTool.Call);

		var systemInstructions =
			$"""
			Your job is to create an itinerary for the person.
			
			Each day needs an activity, hotel and restaurant.
			
			Always use the findPointsOfInterest tool to find businesses 
			and activities in {landmark.Name}, especially hotels
			and restaurants.
			
			The point of interest categories may include:

			{string.Join(", ", Enum.GetNames<FindPointsOfInterestTool.Category>())}
			
			Here is a description of {landmark.Name} for your reference
			when considering what activities to generate:
			{landmark.Description}
			""";

		var userPrompt =
			$"""
			Generate a {dayCount}-day itinerary to {landmark.Name}.
			
			Give it a fun title and description.

			Here is an example, but don't copy it:
			{JsonSerializer.Serialize(Itinerary.GetExampleTripToJapan(), new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase })}
			""";

		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, systemInstructions),
			new(ChatRole.User, userPrompt)
		};

		var options = new ChatOptions
		{
			Tools = [findPointsOfInterestFunction],
			ResponseFormat = ChatResponseFormat.ForJsonSchema(
				Itinerary.ToJsonSchema(landmarkService.Landmarks.Select(l => l.Name)),
				schemaName: "Itinerary",
				schemaDescription: "A travel itinerary with days and activities")
		};

		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		var deserializer = new StreamingJsonDeserializer<Itinerary>(jsonOptions);

		var bufferedClient = new BufferedChatClient(chatClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
		await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages, options, cancellationToken))
		{
			// Detect tool calls from the streaming update
			foreach (var item in update.Contents)
			{
				if (item is FunctionCallContent functionCall)
				{
					var toolLookup = new ToolLookup
					{
						Id = functionCall.CallId,
						Arguments = functionCall.Arguments
					};

					yield return new ItineraryStreamUpdate { ToolLookup = toolLookup };
				}
				else if (item is FunctionResultContent functionResult)
				{
					var toolLookup = new ToolLookup
					{
						Id = functionResult.CallId,
						Result = functionResult.Result
					};

					yield return new ItineraryStreamUpdate { ToolLookupResult = toolLookup };
				}
				else if (item is TextContent textContent)
				{
					var partialItinerary = deserializer.ProcessChunk(textContent.Text);
					if (partialItinerary is not null)
					{
						yield return new ItineraryStreamUpdate { PartialItinerary = partialItinerary };
					}
				}
			}
		}
	}
}
