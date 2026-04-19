using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.AI;

/// <summary>
/// A delegating chat client that enables tool/function calling for models that support
/// the Phi-4 tool calling token format but expose only a text-in/text-out API.
///
/// When tools are present in <see cref="ChatOptions.Tools"/>, this client:
/// 1. Injects tool definitions into the system prompt using Phi-4 native token format
/// 2. Parses <c>&lt;|tool_call|&gt;</c> blocks from the model's text response
/// 3. Converts parsed tool calls to <see cref="FunctionCallContent"/> for
///    <c>FunctionInvokingChatClient</c> to invoke
///
/// Pipeline: FICC → PromptBasedToolCallingClient → PhiSilicaChatClient
/// </summary>
public sealed class PromptBasedToolCallingClient : DelegatingChatClient
{
	// Regex to find tool call blocks in model output.
	// Matches plain-text markers: <tool_call>...</tool_call>
	// Also matches Phi-4 native tokens if they survive: <|tool_call|>...<|/tool_call|>
	// Uses greedy match to capture full JSON including nested braces
	private static readonly Regex ToolCallRegex = new(
		@"<\|?tool_call\|?>\s*(\{.*\})\s*<\|?/?tool_call\|?>",
		RegexOptions.Singleline | RegexOptions.Compiled);

	public PromptBasedToolCallingClient(IChatClient inner) : base(inner) { }

	public override async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		bool hadTools = options?.Tools is { Count: > 0 };
		(messages, options) = RewriteIfNeeded(messages, options);
		var response = await base.GetResponseAsync(messages, options, cancellationToken);

		if (hadTools)
		{
			ParseToolCallsFromResponse(response);
		}

		return response;
	}

	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		bool hadTools = options?.Tools is { Count: > 0 };
		(messages, options) = RewriteIfNeeded(messages, options);

		if (!hadTools)
		{
			// No tools — pass through directly
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
			{
				yield return update;
			}
			yield break;
		}

		// Buffer streaming output to detect tool calls in the complete text
		var updates = new List<ChatResponseUpdate>();
		var fullText = new StringBuilder();

		await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
		{
			updates.Add(update);
			foreach (var content in update.Contents)
			{
				if (content is TextContent tc && tc.Text is not null)
					fullText.Append(tc.Text);
			}
		}

		// Check if the complete text contains tool calls
		var toolCalls = ExtractToolCalls(fullText.ToString());
		if (toolCalls.Count > 0)
		{
			// Yield a single update with FunctionCallContent instead of text
			var toolUpdate = new ChatResponseUpdate
			{
				Role = ChatRole.Assistant,
			};

			foreach (var (name, args) in toolCalls)
			{
				toolUpdate.Contents.Add(new FunctionCallContent(
					callId: Guid.NewGuid().ToString("N")[..8],
					name: name,
#pragma warning disable IL3050, IL2026
					arguments: args != null ? JsonSerializer.Deserialize<Dictionary<string, object?>>(args) : null));
#pragma warning restore IL3050, IL2026
			}

			yield return toolUpdate;
		}
		else
		{
			// No tool calls found — yield original updates
			foreach (var update in updates)
			{
				yield return update;
			}
		}
	}

	private static (IEnumerable<ChatMessage>, ChatOptions?) RewriteIfNeeded(
		IEnumerable<ChatMessage> messages, ChatOptions? options)
	{
		if (options?.Tools is not { Count: > 0 })
			return (messages, options);

		var tools = options.Tools.OfType<AIFunction>().ToList();
		if (tools.Count == 0)
			return (messages, options);

		// Build tool definitions as JSON array
		var toolDefs = new StringBuilder();
		toolDefs.Append('[');
		for (int i = 0; i < tools.Count; i++)
		{
			if (i > 0) toolDefs.Append(',');
			toolDefs.Append('{');
			toolDefs.Append($"\"name\":\"{tools[i].Name}\"");
			toolDefs.Append($",\"description\":\"{EscapeJson(tools[i].Description)}\"");
			toolDefs.Append($",\"parameters\":{tools[i].JsonSchema}");
			toolDefs.Append('}');
		}
		toolDefs.Append(']');

		// Create system prompt with tool definitions.
		// We use plain-text markers (<tool_call>/<tool_call>) because the Windows
		// LanguageModel API strips Phi-4 special tokens (<|tool_call|>) from output.
		var toolSystemPrompt = new ChatMessage(ChatRole.System,
			"You are a helpful assistant with access to the following tools:\n\n" +
			$"{toolDefs}\n\n" +
			"When the user asks a question that requires using a tool, you MUST respond with ONLY a tool call in this EXACT format (no other text):\n\n" +
			"<tool_call>{\"name\": \"ToolName\", \"arguments\": {\"param\": \"value\"}}</tool_call>\n\n" +
			"Rules:\n" +
			"- Respond with ONLY the <tool_call> block when calling a tool, no other text before or after.\n" +
			"- Use the exact function name and parameter names from the tool definitions.\n" +
			"- The arguments must be valid JSON matching the parameter schema.\n" +
			"- After receiving a tool result, use it to formulate your final response to the user.\n" +
			"- If the user's question can be answered without tools, respond normally.");

		// Prepend tool system message
		messages = [toolSystemPrompt, .. messages];

		// Clone options and remove tools so the inner client doesn't see them
		// (the inner client doesn't support tools natively)
		options = options.Clone();
		options.Tools = null;

		return (messages, options);
	}

	private static void ParseToolCallsFromResponse(ChatResponse response)
	{
		foreach (var message in response.Messages)
		{
			var toolCalls = new List<(string Name, string? Args)>();
			var hasTextWithToolCall = false;

			foreach (var content in message.Contents)
			{
				if (content is TextContent tc && tc.Text is not null)
				{
					var calls = ExtractToolCalls(tc.Text);
					if (calls.Count > 0)
					{
						toolCalls.AddRange(calls);
						hasTextWithToolCall = true;
					}
				}
			}

			if (hasTextWithToolCall && toolCalls.Count > 0)
			{
				// Replace text content with FunctionCallContent
				// Remove the text that contained tool calls
				for (int i = message.Contents.Count - 1; i >= 0; i--)
				{
					if (message.Contents[i] is TextContent tc && tc.Text is not null && ToolCallRegex.IsMatch(tc.Text))
					{
						// Strip the tool call from text; if nothing remains, remove the content
						var remaining = ToolCallRegex.Replace(tc.Text, "").Trim();
						if (string.IsNullOrEmpty(remaining))
							message.Contents.RemoveAt(i);
						else
							tc.Text = remaining;
					}
				}

				// Add FunctionCallContent entries
				foreach (var (name, args) in toolCalls)
				{
					message.Contents.Add(new FunctionCallContent(
						callId: Guid.NewGuid().ToString("N")[..8],
						name: name,
#pragma warning disable IL3050, IL2026
						arguments: args != null ? JsonSerializer.Deserialize<Dictionary<string, object?>>(args) : null));
#pragma warning restore IL3050, IL2026
				}
			}
		}
	}

	private static List<(string Name, string? Args)> ExtractToolCalls(string text)
	{
		var results = new List<(string Name, string? Args)>();

		// Try the primary regex first (with closing tag)
		foreach (Match match in ToolCallRegex.Matches(text))
		{
			TryParseToolCall(match.Groups[1].Value.Trim(), results);
		}

		// Fallback: if no matches with closing tag, try finding JSON after <tool_call> without closing tag
		if (results.Count == 0)
		{
			var fallbackRegex = new Regex(@"<\|?tool_call\|?>\s*(\{.*)", RegexOptions.Singleline);
			foreach (Match match in fallbackRegex.Matches(text))
			{
				var jsonCandidate = match.Groups[1].Value.Trim();
				// Try to extract a balanced JSON object
				var extracted = ExtractBalancedJson(jsonCandidate);
				if (extracted is not null)
				{
					TryParseToolCall(extracted, results);
				}
			}
		}

		return results;
	}

	private static void TryParseToolCall(string json, List<(string Name, string? Args)> results)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
			var args = root.TryGetProperty("arguments", out var argsProp) ? argsProp.GetRawText() : null;

			if (!string.IsNullOrEmpty(name))
			{
				results.Add((name!, args));
			}
		}
		catch (JsonException)
		{
			// Model output wasn't valid JSON — skip
		}
	}

	/// <summary>
	/// Extracts a balanced JSON object from a string that may have trailing content.
	/// </summary>
	private static string? ExtractBalancedJson(string text)
	{
		if (string.IsNullOrEmpty(text) || text[0] != '{')
			return null;

		int depth = 0;
		bool inString = false;
		bool escaped = false;

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];

			if (escaped)
			{
				escaped = false;
				continue;
			}

			if (c == '\\' && inString)
			{
				escaped = true;
				continue;
			}

			if (c == '"')
			{
				inString = !inString;
				continue;
			}

			if (!inString)
			{
				if (c == '{') depth++;
				else if (c == '}')
				{
					depth--;
					if (depth == 0)
						return text[..(i + 1)];
				}
			}
		}

		return null; // Unbalanced
	}

	private static string EscapeJson(string value)
	{
		return value
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal)
			.Replace("\n", "\\n", StringComparison.Ordinal)
			.Replace("\r", "\\r", StringComparison.Ordinal)
			.Replace("\t", "\\t", StringComparison.Ordinal);
	}
}
