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

	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		bool hadSchema = options?.ResponseFormat is ChatResponseFormatJson;
		(messages, options) = RewriteIfNeeded(messages, options);

		if (!hadSchema)
		{
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
				yield return update;
			yield break;
		}

		// Smart buffering: peek at initial tokens to detect code fences.
		// If no fences, flush buffer and stream the rest through directly.
		// If fences, buffer everything and strip at the end.
		var buffer = new List<ChatResponseUpdate>();
		var initialText = new System.Text.StringBuilder();
		bool fenceDetected = false;
		bool decisionMade = false;

		await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
		{
			if (!decisionMade)
			{
				buffer.Add(update);
				foreach (var c in update.Contents)
					if (c is TextContent tc && tc.Text is not null)
						initialText.Append(tc.Text);

				var soFar = initialText.ToString().TrimStart();
				if (soFar.Length >= 3)
				{
					decisionMade = true;
					if (soFar.StartsWith("```", StringComparison.Ordinal))
					{
						fenceDetected = true;
					}
					else
					{
						foreach (var b in buffer) yield return b;
						buffer.Clear();
					}
				}
			}
			else if (fenceDetected)
			{
				buffer.Add(update);
				foreach (var c in update.Contents)
					if (c is TextContent tc && tc.Text is not null)
						initialText.Append(tc.Text);
			}
			else
			{
				yield return update;
			}
		}

		if (fenceDetected || !decisionMade)
		{
			var fullText = new System.Text.StringBuilder();
			ChatRole? role = null;
			foreach (var u in buffer)
			{
				role ??= u.Role;
				foreach (var c in u.Contents)
					if (c is TextContent tc && tc.Text is not null)
						fullText.Append(tc.Text);
			}
			yield return new ChatResponseUpdate
			{
				Role = role ?? ChatRole.Assistant,
				Contents = [new TextContent(StripCodeFencesFromText(fullText.ToString()))]
			};
		}
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
					tc.Text = StripCodeFencesFromText(text);
				}
			}
		}
	}

	internal static string StripCodeFencesFromText(string text)
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

		return trimmed.Trim();
	}
}
