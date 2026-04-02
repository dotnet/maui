using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class TaggingService(IChatClient chatClient)
{
	public async Task<List<string>> GenerateTagsAsync(string text, CancellationToken cancellationToken = default)
	{
		var systemPrompt =
			"""
			You are a social media assistant that creates engaging travel hashtags.

			Given text about a travel destination, generate 5 hashtags that are fun
			and playful but still relevant to the destination.
			Keep them recognizable and useful — mix the destination name with travel
			vibes (e.g. #ParisVibes, #ExploreRome, #BeachDayBali, #TokyoNights, #HikingPatagonia).

			Return ONLY a comma-separated list of 5 hashtags. No explanations.
			""";

		var userPrompt =
			$"""
			Create 5 fun, catchy hashtags for this destination:
			
			{text}
			""";

		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, systemPrompt),
			new(ChatRole.User, userPrompt)
		};

		var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
		var responseText = response.Text ?? string.Empty;

		return responseText
			.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(tag => tag.TrimStart('#', '-', '•', '*').Trim())
			.Where(tag => tag.Length > 1 && !tag.Any(char.IsWhiteSpace))
			.Distinct()
			.Take(5)
			.ToList();
	}
}
