using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Single wrapper for PhiSilicaChatClient that handles structured output AND tool calling.
/// Temporary until the LanguageModel API supports these natively.
///
/// Handles all combinations:
/// - Tools only → structured JSON tool call schema
/// - ResponseFormat only → schema injected as prompt instructions
/// - Tools + ResponseFormat → combined schema (tool_call OR user's structured response)
/// - Neither → pass through
///
/// Usage: new PhiSilicaToolsAndSchemaClient(new PhiSilicaChatClient())
/// </summary>
public sealed class PhiSilicaToolsAndSchemaClient : DelegatingChatClient
{
	private const string PendingToolKey = "__pending_next_step";

	public PhiSilicaToolsAndSchemaClient(IChatClient inner) : base(inner) { }

	public override async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var (rewritten, newOptions) = Rewrite(messages, options);
		var response = await base.GetResponseAsync(rewritten, newOptions, cancellationToken);
		StripCodeFences(response);

		if (options?.Tools is { Count: > 0 })
			ConvertToolCallResponse(response);

		return response;
	}

	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (options?.Tools is { Count: > 0 })
		{
			// Tool path: delegate to non-streaming for reliability
			var response = await GetResponseAsync(messages, options, cancellationToken);
			foreach (var msg in response.Messages)
				yield return new ChatResponseUpdate { Role = msg.Role, Contents = [.. msg.Contents] };
			yield break;
		}

		// Schema or plain streaming with smart code fence detection
		bool hadSchema = options?.ResponseFormat is ChatResponseFormatJson;
		var (rewritten, newOptions) = Rewrite(messages, options);

		if (!hadSchema)
		{
			await foreach (var update in base.GetStreamingResponseAsync(rewritten, newOptions, cancellationToken))
				yield return update;
			yield break;
		}

		// Smart buffer: peek at first tokens for code fences
		var buffer = new List<ChatResponseUpdate>();
		var initialText = new StringBuilder();
		bool fenceDetected = false, decided = false;

		await foreach (var update in base.GetStreamingResponseAsync(rewritten, newOptions, cancellationToken))
		{
			if (!decided)
			{
				buffer.Add(update);
				foreach (var c in update.Contents)
					if (c is TextContent tc && tc.Text is not null) initialText.Append(tc.Text);
				if (initialText.ToString().TrimStart().Length >= 3)
				{
					decided = true;
					if (initialText.ToString().TrimStart().StartsWith("```", StringComparison.Ordinal))
						fenceDetected = true;
					else { foreach (var b in buffer) yield return b; buffer.Clear(); }
				}
			}
			else if (fenceDetected)
			{
				buffer.Add(update);
				foreach (var c in update.Contents)
					if (c is TextContent tc && tc.Text is not null) initialText.Append(tc.Text);
			}
			else yield return update;
		}

		if (fenceDetected || !decided)
		{
			var full = new StringBuilder();
			ChatRole? role = null;
			foreach (var u in buffer) { role ??= u.Role; foreach (var c in u.Contents) if (c is TextContent tc && tc.Text is not null) full.Append(tc.Text); }
			yield return new ChatResponseUpdate { Role = role ?? ChatRole.Assistant, Contents = [new TextContent(StripText(full.ToString()))] };
		}
	}

	// ═══════════════════════════════════════════════════════════
	// REWRITE — single entry point for all cases
	// ═══════════════════════════════════════════════════════════

	private (IEnumerable<ChatMessage>, ChatOptions?) Rewrite(
		IEnumerable<ChatMessage> messages, ChatOptions? options)
	{
		bool hasTools = options?.Tools is { Count: > 0 };
		var tools = hasTools ? options!.Tools!.OfType<AIFunction>().ToList() : null;
		bool hasSchema = options?.ResponseFormat is ChatResponseFormatJson;

		if (hasTools && tools?.Count > 0)
			return RewriteForTools(messages, options!, tools);

		if (hasSchema)
			return RewriteForSchema(messages, options!);

		return (messages, options);
	}

	// ═══════════════════════════════════════════════════════════
	// SCHEMA-ONLY REWRITE
	// ═══════════════════════════════════════════════════════════

	private static (IEnumerable<ChatMessage>, ChatOptions) RewriteForSchema(
		IEnumerable<ChatMessage> messages, ChatOptions options)
	{
		if (options.ResponseFormat is not ChatResponseFormatJson { Schema: { } schema })
			return (messages, options);

		var prompt = new ChatMessage(ChatRole.System, $$"""
			IMPORTANT: Your response must be a single valid JSON object with real values.
			Do NOT wrap the response in markdown code fences or backticks.
			Do NOT include "$schema", "type", "properties", "required", or "description" keys from the schema definition.
			Do NOT echo the schema back. Only output the data.
			For enum values, use EXACTLY the values listed in the schema.

			JSON schema for the expected response:
			{{schema}}
			""");

		var newOptions = options.Clone();
		newOptions.ResponseFormat = null;
		return ([prompt, .. messages], newOptions);
	}

	// ═══════════════════════════════════════════════════════════
	// TOOL CALLING REWRITE (builds schema prompt directly, no intermediate ResponseFormat)
	// ═══════════════════════════════════════════════════════════

	private (IEnumerable<ChatMessage>, ChatOptions) RewriteForTools(
		IEnumerable<ChatMessage> messages, ChatOptions options, List<AIFunction> tools)
	{
		// Build tool descriptions with enum hints
		var toolDesc = new StringBuilder();
		foreach (var tool in tools)
		{
			toolDesc.AppendLine($"- {tool.Name}: {tool.Description}");
			toolDesc.AppendLine($"  Parameters: {tool.JsonSchema}");
			try
			{
				using var sd = JsonDocument.Parse(tool.JsonSchema.GetRawText());
				if (sd.RootElement.TryGetProperty("properties", out var props))
					foreach (var p in props.EnumerateObject())
						if (p.Value.TryGetProperty("enum", out var ev))
							toolDesc.AppendLine($"  IMPORTANT: {p.Name} must be EXACTLY one of: {string.Join(", ", ev.EnumerateArray().Select(v => v.GetString()))}");
			}
			catch { }
		}

		// Build the tool call decision schema directly as text (no ResponseFormat)
		var userSchema = options.ResponseFormat is ChatResponseFormatJson { Schema: { } s } ? s : (JsonElement?)null;
		var schema = BuildToolCallSchema(tools, userSchema);

		var responseHint = userSchema != null
			? "If you can answer directly, set type to \"response\" and fill in the response object matching the schema."
			: "If you can answer directly without a tool, set type to \"text\" and put your answer in the text field.";

		// Build the FULL prompt including both tool instructions AND schema instructions
		var systemPrompt = new ChatMessage(ChatRole.System,
			"You are a helpful assistant with access to tools.\n\n" +
			$"Available tools:\n{toolDesc}\n" +
			$"Your response MUST be a single valid JSON object matching this schema:\n{schema}\n\n" +
			"Do NOT wrap the response in markdown code fences or backticks.\n" +
			"If the user's question requires a tool, set type to \"tool_call\", set tool_name, and provide arguments.\n" +
			$"{responseHint}\n" +
			"Call only ONE tool at a time. After receiving the result, you may call another.\n" +
			"For enum parameters, use EXACTLY one of the allowed values listed above.\n" +
			"If a tool has no required parameters, use an empty arguments object {}.\n" +
			"If you will need to call another tool AFTER this one, set next_step to describe the plan.\n" +
			"When you see a tool result in the conversation, check if you still need to call another tool.");

		// Build message list with pending plan detection
		var allMessages = new List<ChatMessage> { systemPrompt };
		string? pendingPlan = null;
		bool sawResult = false;

		foreach (var msg in messages)
		{
			allMessages.Add(msg);
			foreach (var c in msg.Contents)
			{
				if (c is FunctionCallContent fcc && fcc.AdditionalProperties?.TryGetValue(PendingToolKey, out var pt) == true && pt is string ps)
				{ pendingPlan = ps; sawResult = false; }
				else if (c is FunctionResultContent && pendingPlan != null)
					sawResult = true;
			}
		}

		if (pendingPlan != null && sawResult)
		{
			allMessages.Add(new ChatMessage(ChatRole.System,
				$"REMINDER: Your earlier plan was: \"{pendingPlan}\". " +
				"The tool result is above. Now call the next tool using data from the result."));
		}

		// Clone options: remove tools AND ResponseFormat (we handle both via prompt)
		var newOptions = options.Clone();
		newOptions.Tools = null;
		newOptions.ResponseFormat = null;

		return (allMessages, newOptions);
	}

	private static JsonElement BuildToolCallSchema(List<AIFunction> tools, JsonElement? userSchema)
	{
		var toolNames = tools.Select(t => $"\"{t.Name}\"").ToList();
		var nextStep = tools.Count >= 2
			? ",\"next_step\":{\"type\":\"string\",\"description\":\"Optional plan for the next tool call\"}"
			: "";

		string responseField;
		string typeEnum;
		if (userSchema != null)
		{
			responseField = $",\"response\":{userSchema.Value.GetRawText()}";
			typeEnum = "[\"tool_call\",\"response\"]";
		}
		else
		{
			responseField = ",\"text\":{\"type\":\"string\",\"description\":\"Your text response\"}";
			typeEnum = "[\"tool_call\",\"text\"]";
		}

		var json = $$"""{"type":"object","properties":{"type":{"type":"string","enum":{{typeEnum}}},"tool_name":{"type":"string","enum":[{{string.Join(",", toolNames)}}]},"arguments":{"type":"object"}{{responseField}}{{nextStep}}},"required":["type"]}""";
		return JsonDocument.Parse(json).RootElement.Clone();
	}

	// ═══════════════════════════════════════════════════════════
	// RESPONSE PARSING
	// ═══════════════════════════════════════════════════════════

	private static void ConvertToolCallResponse(ChatResponse response)
	{
		foreach (var message in response.Messages)
		{
			var text = string.Join("", message.Contents.OfType<TextContent>().Select(tc => tc.Text));
			if (string.IsNullOrEmpty(text)) continue;

			var parsed = TryParse(text);
			if (parsed is ToolCall tc2)
			{
				message.Contents.Clear();
				var fcc = new FunctionCallContent(
					Guid.NewGuid().ToString("N")[..16], tc2.Name,
#pragma warning disable IL3050, IL2026
					tc2.Args != null ? JsonSerializer.Deserialize<Dictionary<string, object?>>(tc2.Args.Value.GetRawText()) : null);
#pragma warning restore IL3050, IL2026
				if (!string.IsNullOrWhiteSpace(tc2.NextStep))
					(fcc.AdditionalProperties ??= new AdditionalPropertiesDictionary())[PendingToolKey] = tc2.NextStep;
				message.Contents.Add(fcc);
			}
			else if (parsed is TextResp tr)
			{
				message.Contents.Clear();
				message.Contents.Add(new TextContent(tr.Text));
			}
		}
	}

	private static object? TryParse(string text)
	{
		var t = StripText(text);
		try
		{
			using var doc = JsonDocument.Parse(t);
			var r = doc.RootElement;
			var type = r.TryGetProperty("type", out var tp) ? tp.GetString() : null;

			if (type == "tool_call")
			{
				var name = r.TryGetProperty("tool_name", out var np) ? np.GetString() : null;
				var args = r.TryGetProperty("arguments", out var ap) ? ap.Clone() : (JsonElement?)null;
				var next = r.TryGetProperty("next_step", out var ns) && ns.ValueKind == JsonValueKind.String ? ns.GetString() : null;
				if (!string.IsNullOrEmpty(name)) return new ToolCall(name!, args, next);
			}
			else if (type == "text")
				return new TextResp(r.TryGetProperty("text", out var tp2) ? tp2.GetString() ?? "" : "");
			else if (type == "response" && r.TryGetProperty("response", out var rp))
				return new TextResp(rp.GetRawText());
		}
		catch (JsonException) { }
		return null;
	}

	// ═══════════════════════════════════════════════════════════
	// UTILITIES
	// ═══════════════════════════════════════════════════════════

	private static void StripCodeFences(ChatResponse response)
	{
		foreach (var msg in response.Messages)
			foreach (var c in msg.Contents)
				if (c is TextContent tc && tc.Text is { } txt)
					tc.Text = StripText(txt);
	}

	internal static string StripText(string text)
	{
		var s = text.Trim();
		if (s.StartsWith("```", StringComparison.Ordinal))
		{ var nl = s.IndexOf('\n', StringComparison.Ordinal); if (nl > 0) s = s[(nl + 1)..]; }
		if (s.EndsWith("```", StringComparison.Ordinal)) s = s[..^3];
		return s.Trim();
	}

	private record ToolCall(string Name, JsonElement? Args, string? NextStep);
	private record TextResp(string Text);
}
