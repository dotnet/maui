using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class TaggingService(IChatClient chatClient)
{
    public async Task<List<string>> GenerateTagsAsync(string text, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant that extracts relevant tags from text. Return only a JSON array of 3-5 tag strings."),
            new(ChatRole.User, $"Extract relevant tags from this text: {text}")
        };

        var options = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json
        };

        var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
        var jsonText = response.ToString();
        
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonText) ?? new List<string>();
    }
}
