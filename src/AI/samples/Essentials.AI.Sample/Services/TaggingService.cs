using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class TaggingService(IChatClient chatClient)
{
	public async Task<List<string>> GenerateTagsAsync(string text, CancellationToken cancellationToken = default)
	{
		var systemPrompt =
			"""
			Your job is to extract the most relevant tags from the input text.
			""";

		var userPrompt =
			$"""
			Extract relevant tags from this text:
			
			{text}
			""";

		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, systemPrompt),
			new(ChatRole.User, userPrompt)
		};

		var response = await chatClient.GetResponseAsync<TaggingResponse>(messages, cancellationToken: cancellationToken);
		var jsonText = response.ToString();

		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		return JsonSerializer.Deserialize<TaggingResponse>(jsonText, jsonOptions)?.Tags ?? [];
	}

	public class TaggingResponse
	{
		[Description("Most important topics in the input text.")]
		[Length(5, 5)]
		public List<string> Tags { get; set; } = [];
	}
}
