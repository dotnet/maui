using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class ItineraryService(IChatClient chatClient)
{
    public record ItineraryStreamUpdate(string? ToolLookup = null, Itinerary? PartialItinerary = null);

    public async IAsyncEnumerable<ItineraryStreamUpdate> StreamItineraryAsync(
        Landmark landmark, 
        int dayCount,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var toolLookups = new Dictionary<string, int>();

        var findPointsOfInterestTool = new FindPointsOfInterestTool(landmark, toolLookups);
        var findPointsOfInterestFunction = AIFunctionFactory.Create(findPointsOfInterestTool.Call);

        var systemInstructions = 
            "Your job is to create an itinerary for the person.\n\n" +
            "Each day needs an activity, hotel and restaurant.\n\n" +
            $"Always use the findPointsOfInterest tool to find businesses and activities in {landmark.Name}, especially hotels and restaurants.\n\n" +
            "The point of interest categories may include:\n" +
            string.Join(", ", FindPointsOfInterestTool.Categories) + "\n\n" +
            $"Here is a description of {landmark.Name} for your reference when considering what activities to generate:\n" +
            landmark.Description;

        var userPrompt = 
            $"Generate a {dayCount}-day itinerary to {landmark.Name}.\n\n" +
            "Give it a fun title and description.\n\n" +
            "Here is an example, but don't copy it:\n" +
            JsonSerializer.Serialize(Itinerary.GetExample());

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemInstructions),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Tools = [ findPointsOfInterestFunction ],
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                Itinerary.GetJsonSchema(),
                schemaName: "Itinerary",
                schemaDescription: "A travel itinerary with days and activities"
            )
        };

        var schema = Itinerary.GetJsonSchema().ToString();


        var contentBuilder = new StringBuilder();

        var update = await chatClient.GetResponseAsync(messages, options, cancellationToken);
        // await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            foreach (var toolLookup in toolLookups.Keys)
            {
                yield return new ItineraryStreamUpdate { ToolLookup = toolLookup };
                toolLookups[toolLookup] = 0;
            }

            if (update.Text is not null)
            {
                contentBuilder.Append(update.Text);
                
                var jsonText = contentBuilder.ToString().Trim();
                if (jsonText.StartsWith("{", StringComparison.Ordinal) && jsonText.Contains("\"Days\"", StringComparison.Ordinal))
                {
                    var partialItinerary = TryDeserializeItinerary(jsonText);
                    if (partialItinerary is not null)
                    {
                        yield return new ItineraryStreamUpdate { PartialItinerary = partialItinerary };
                    }
                }
            }
        }
    }

    private static Itinerary? TryDeserializeItinerary(string jsonText)
    {
        try
        {
            return JsonSerializer.Deserialize<Itinerary>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

