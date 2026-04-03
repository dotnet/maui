using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// A delegating chat client that converts native JSON schema response format requests
/// into prompt-based instructions. Use this for models (like Phi Silica) that don't
/// support structured output natively — the schema is injected into the prompt instead.
/// Also strips markdown code fences from responses so JSON parsing succeeds.
/// </summary>
public sealed class PromptBasedSchemaClient : DelegatingChatClient
{
	public PromptBasedSchemaClient(IChatClient inner) : base(inner) { }

	public override async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		(messages, options) = RewriteIfNeeded(messages, options);
		var response = await base.GetResponseAsync(messages, options, cancellationToken);
		StripCodeFences(response);
		return response;
	}

	public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		(messages, options) = RewriteIfNeeded(messages, options);
		return base.GetStreamingResponseAsync(messages, options, cancellationToken);
	}

	static (IEnumerable<ChatMessage>, ChatOptions?) RewriteIfNeeded(
		IEnumerable<ChatMessage> messages, ChatOptions? options)
	{
		if (options?.ResponseFormat is ChatResponseFormatJson jsonFormat && jsonFormat.Schema is { } schema)
		{
			var schemaPrompt = new ChatMessage(ChatRole.System, $$"""
				IMPORTANT: Your response must be a single valid JSON object with real values.
				Do NOT wrap the response in markdown code fences or backticks.
				Do NOT include "$schema", "type", "properties", "required", or "description" keys from the schema definition.
				Do NOT echo the schema back. Only output the data.
				For enum values, use EXACTLY the values listed in the schema (e.g. "Sightseeing" not "Sight seeing", "FoodAndDining" not "Food and Dining").

				JSON schema for the expected response:
				{{schema}}
				""");

			// Prepend as system message so it's authoritative
			messages = [schemaPrompt, .. messages];

			options = options.Clone();
			options.ResponseFormat = null;
		}

		return (messages, options);
	}

	/// <summary>
	/// Strips markdown code fences (```json ... ```) from the response text
	/// so JSON deserialization succeeds.
	/// </summary>
	static void StripCodeFences(ChatResponse response)
	{
		foreach (var message in response.Messages)
		{
			foreach (var content in message.Contents)
			{
				if (content is TextContent tc && tc.Text is { } text)
				{
					var trimmed = text.Trim();
					// Strip ```json ... ``` or ``` ... ```
					if (trimmed.StartsWith("```", StringComparison.Ordinal))
					{
						var firstNewline = trimmed.IndexOf('\n', StringComparison.Ordinal);
						if (firstNewline > 0)
							trimmed = trimmed[(firstNewline + 1)..];
					}
					if (trimmed.EndsWith("```", StringComparison.Ordinal))
						trimmed = trimmed[..^3];

					tc.Text = trimmed.Trim();
				}
			}
		}
	}
}
