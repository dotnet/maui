using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

/// <summary>
/// A delegating chat client that enables tool/function calling by leveraging
/// structured output (JSON schema). Instead of asking the model to output
/// special tags like &lt;tool_call&gt;, this client asks the model to produce
/// a structured JSON response that IS a tool call decision.
///
/// This approach is more reliable than tag-based parsing because:
/// 1. Structured output already works perfectly with Phi Silica
/// 2. The model follows JSON schema constraints (including enums) reliably
/// 3. No regex parsing needed — just JSON deserialization
/// 4. The schema enforces the exact tool names and parameter types
///
/// Pipeline: FICC → StructuredToolCallingClient → PromptBasedSchemaClient → PhiSilicaChatClient
/// </summary>
public sealed class StructuredToolCallingClient : DelegatingChatClient
{
	private const string PendingToolKey = "__pending_next_tool";

	public StructuredToolCallingClient(IChatClient inner) : base(inner) { }

	public override async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		if (options?.Tools is not { Count: > 0 })
			return await base.GetResponseAsync(messages, options, cancellationToken);

		var tools = options.Tools.OfType<AIFunction>().ToList();
		if (tools.Count == 0)
			return await base.GetResponseAsync(messages, options, cancellationToken);

		// Build a JSON schema that represents "either call a tool OR respond with text"
		var (rewrittenMessages, rewrittenOptions) = RewriteAsStructuredOutput(messages, options, tools);

		// If the previous assistant turn left a planned next step (stashed on the
		// FunctionCallContent we synthesized), inject a reminder so the 3.8B model
		// follows through with the second tool call instead of replying with text.
		rewrittenMessages = InjectPendingPlanReminder(rewrittenMessages, messages);

		var response = await base.GetResponseAsync(rewrittenMessages, rewrittenOptions, cancellationToken);

		// Parse the structured JSON response into FunctionCallContent or plain text
		return ConvertStructuredResponse(response);
	}

	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (options?.Tools is not { Count: > 0 })
		{
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
				yield return update;
			yield break;
		}

		// When tools are present, use the non-streaming path for reliability.
		// PhiSilicaChatClient.GetResponseAsync delegates to streaming internally anyway,
		// so there's no latency penalty. The non-streaming path through PromptBasedSchemaClient
		// strips code fences and produces cleaner JSON for parsing.
		var response = await GetResponseAsync(messages, options, cancellationToken);

		foreach (var message in response.Messages)
		{
			var update = new ChatResponseUpdate
			{
				Role = message.Role,
			};

			foreach (var content in message.Contents)
			{
				update.Contents.Add(content);
			}

			yield return update;
		}
	}

	private static (IEnumerable<ChatMessage>, ChatOptions) RewriteAsStructuredOutput(
		IEnumerable<ChatMessage> messages,
		ChatOptions options,
		List<AIFunction> tools)
	{
		// Build a dynamic JSON schema for the tool call decision.
		// If the user also requested structured output (ResponseFormat), embed their
		// schema as the "response" field instead of a plain "text" string.
		var userSchema = options.ResponseFormat is ChatResponseFormatJson { Schema: { } s } ? s : (JsonElement?)null;
		var schema = BuildToolCallSchema(tools, userSchema);

		// Build system prompt explaining the tools and the structured output format
		var toolDescriptions = new StringBuilder();
		foreach (var tool in tools)
		{
			toolDescriptions.AppendLine($"- {tool.Name}: {tool.Description}");
			toolDescriptions.AppendLine($"  Parameters: {tool.JsonSchema}");

			// Extract and inline enum values for better adherence
			try
			{
				using var schemaDoc = JsonDocument.Parse(tool.JsonSchema.GetRawText());
				if (schemaDoc.RootElement.TryGetProperty("properties", out var props))
				{
					foreach (var prop in props.EnumerateObject())
					{
						if (prop.Value.TryGetProperty("enum", out var enumValues))
						{
							var values = string.Join(", ", enumValues.EnumerateArray().Select(v => v.GetString()));
							toolDescriptions.AppendLine($"  IMPORTANT: {prop.Name} must be EXACTLY one of: {values}");
						}
					}
				}
			}
			catch { /* Schema parsing failed — continue without enum hints */ }
		}

		var responseHint = userSchema != null
			? "If you can answer directly, set type to \"response\" and fill in the response object matching the schema."
			: "If you can answer directly without a tool, set type to \"text\" and put your answer in the text field.";

		var systemPrompt = new ChatMessage(ChatRole.System,
			"You are a helpful assistant with access to tools.\n\n" +
			$"Available tools:\n{toolDescriptions}\n" +
			"Your response MUST be a JSON object matching the provided schema.\n" +
			"If the user's question requires a tool, set type to \"tool_call\", set tool_name, and provide arguments.\n" +
			$"{responseHint}\n" +
			"Call only ONE tool at a time. After receiving the result, you may call another.\n" +
			"For enum parameters, use EXACTLY one of the allowed values listed above.\n" +
			"If a tool has no required parameters, use an empty arguments object {}.\n" +
			"If you will need to call another tool AFTER this one, set next_step to describe the plan.\n" +
			"When you see a tool result in the conversation, check if you still need to call another tool.");

		// Clone options: remove tools, add structured output format
		var newOptions = options.Clone();
		newOptions.Tools = null;
		newOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema(
			schema,
			schemaName: "ToolCallDecision",
			schemaDescription: "Either a tool call or a text response");

		// Check if there's a pending tool from a previous iteration
		// (the model said next_tool=X on its last call, now we remind it)
		var allMessages = new List<ChatMessage> { systemPrompt };
		string? pendingTool = null;
		foreach (var msg in messages)
		{
			allMessages.Add(msg);
			foreach (var content in msg.Contents)
			{
				if (content is FunctionCallContent fcc &&
					fcc.AdditionalProperties?.TryGetValue(PendingToolKey, out var pt) == true &&
					pt is string ptStr)
				{
					pendingTool = ptStr;
				}
			}
		}

		// If there's a pending tool and we've received a tool result, inject a reminder
		if (pendingTool != null && messages.Any(m => m.Contents.Any(c => c is FunctionResultContent)))
		{
			allMessages.Add(new ChatMessage(ChatRole.System,
				$"REMINDER: You previously planned to call {pendingTool} next. " +
				$"The tool result is above. Now call {pendingTool} using the data from the result."));
		}

		return (allMessages, newOptions);
	}

	private static JsonElement BuildToolCallSchema(List<AIFunction> tools, JsonElement? userSchema = null)
	{
		var toolNames = tools.Select(t => $"\"{t.Name}\"").ToList();

		var nextStepProperty = tools.Count >= 2
			? """
				,"next_step": {
					"type": "string",
					"description": "Optional. When a second tool call will be needed after this one, describe the plan here."
				}
			"""
			: "";

		// When the user requested structured output, embed their schema as the
		// "response" field. When type="response", the model fills in this object.
		// When no user schema, use a plain "text" string field.
		string responseField;
		string typeEnum;
		if (userSchema != null)
		{
			responseField = $"""
				,"response": {userSchema.Value.GetRawText()}
			""";
			typeEnum = "[\"tool_call\", \"response\"]";
		}
		else
		{
			responseField = """
				,"text": {
					"type": "string",
					"description": "Your text response (only when type is text)"
				}
			""";
			typeEnum = "[\"tool_call\", \"text\"]";
		}

		var schemaJson = $$"""
		{
			"type": "object",
			"properties": {
				"type": {
					"type": "string",
					"enum": {{typeEnum}},
					"description": "Whether this is a tool call or a direct response"
				},
				"tool_name": {
					"type": "string",
					"enum": [{{string.Join(",", toolNames)}}],
					"description": "The name of the tool to call now"
				},
				"arguments": {
					"type": "object",
					"description": "Arguments for the tool call"
				}{{responseField}}{{nextStepProperty}}
			},
			"required": ["type"]
		}
		""";

		return JsonDocument.Parse(schemaJson).RootElement.Clone();
	}

	private static ChatResponse ConvertStructuredResponse(ChatResponse response)
	{
		foreach (var message in response.Messages)
		{
			var fullText = string.Join("", message.Contents.OfType<TextContent>().Select(tc => tc.Text));
			if (string.IsNullOrEmpty(fullText))
				continue;

			var parsed = TryParseToolCallDecision(fullText);

			if (parsed is ToolCallDecision toolCall)
			{
				// Replace text content with FunctionCallContent
				message.Contents.Clear();
				var fcc = new FunctionCallContent(
					callId: Guid.NewGuid().ToString("N")[..16],
					name: toolCall.ToolName!,
#pragma warning disable IL3050, IL2026
					arguments: toolCall.Arguments != null
						? JsonSerializer.Deserialize<Dictionary<string, object?>>(toolCall.Arguments.Value.GetRawText())
						: null);
#pragma warning restore IL3050, IL2026

				// Stash the model's plan for the NEXT call so we can remind it after the
				// tool result comes back. FICC echoes this assistant message into the
				// follow-up conversation, which is where InjectPendingPlanReminder reads it.
				if (!string.IsNullOrWhiteSpace(toolCall.NextStep))
				{
					(fcc.AdditionalProperties ??= new AdditionalPropertiesDictionary())[PendingToolKey]
						= toolCall.NextStep;
				}

				message.Contents.Add(fcc);
			}
			else if (parsed is TextResponseDecision textResp)
			{
				// Replace with clean text
				message.Contents.Clear();
				message.Contents.Add(new TextContent(textResp.Text));
			}
		}

		return response;
	}

	private static object? TryParseToolCallDecision(string text)
	{
		// Strip code fences if present
		var trimmed = text.Trim();
		if (trimmed.StartsWith("```", StringComparison.Ordinal))
		{
			var nl = trimmed.IndexOf('\n', StringComparison.Ordinal);
			if (nl > 0) trimmed = trimmed[(nl + 1)..];
		}
		if (trimmed.EndsWith("```", StringComparison.Ordinal))
			trimmed = trimmed[..^3];
		trimmed = trimmed.Trim();

		try
		{
			using var doc = JsonDocument.Parse(trimmed);
			var root = doc.RootElement;

			var type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

			if (type == "tool_call")
			{
				var toolName = root.TryGetProperty("tool_name", out var nameProp) ? nameProp.GetString() : null;
				JsonElement? args = root.TryGetProperty("arguments", out var argsProp) ? argsProp.Clone() : null;
				var nextStep = root.TryGetProperty("next_step", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
					? nextProp.GetString()
					: null;

				if (!string.IsNullOrEmpty(toolName))
					return new ToolCallDecision(toolName!, args, nextStep);
			}
			else if (type == "text")
			{
				var textVal = root.TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
				return new TextResponseDecision(textVal ?? "");
			}
			else if (type == "response")
			{
				// Structured response — extract the "response" object as raw JSON text
				if (root.TryGetProperty("response", out var responseProp))
				{
					return new TextResponseDecision(responseProp.GetRawText());
				}
			}
		}
		catch (JsonException)
		{
			// Not valid JSON
		}

		return null;
	}

	private record ToolCallDecision(string ToolName, JsonElement? Arguments, string? NextStep);
	private record TextResponseDecision(string Text);

	// Key used to stash the model's natural-language follow-up plan on a synthesized
	// FunctionCallContent. Lives in AdditionalProperties so it round-trips through
	// FICC's conversation history without affecting tool invocation.

	/// <summary>
	/// If the most recent assistant turn was a tool call we synthesized AND it carried a
	/// next_step plan, AND the new turn now contains a FunctionResultContent (i.e. FICC
	/// has invoked the tool and is asking us again), prepend a system reminder so the
	/// model follows through with the planned second tool call instead of replying with text.
	/// </summary>
	private static IEnumerable<ChatMessage> InjectPendingPlanReminder(
		IEnumerable<ChatMessage> rewrittenMessages,
		IEnumerable<ChatMessage> originalMessages)
	{
		var originalList = originalMessages as IList<ChatMessage> ?? originalMessages.ToList();

		string? pendingPlan = null;
		bool sawResultAfterPlan = false;

		// Walk the history. The plan lives on the most recent assistant FunctionCallContent;
		// after it we need at least one FunctionResultContent before we treat this as a
		// "follow-up turn" worth reminding about.
		foreach (var msg in originalList)
		{
			foreach (var content in msg.Contents)
			{
				if (content is FunctionCallContent fcc)
				{
					if (fcc.AdditionalProperties is { } props
						&& props.TryGetValue(PendingToolKey, out var planObj)
						&& planObj is string planStr
						&& !string.IsNullOrWhiteSpace(planStr))
					{
						pendingPlan = planStr;
						sawResultAfterPlan = false;
					}
					else
					{
						// A later tool call without a plan supersedes any older plan.
						pendingPlan = null;
						sawResultAfterPlan = false;
					}
				}
				else if (content is FunctionResultContent && pendingPlan is not null)
				{
					sawResultAfterPlan = true;
				}
			}
		}

		if (pendingPlan is null || !sawResultAfterPlan)
			return rewrittenMessages;

		var reminder = new ChatMessage(ChatRole.System,
			"REMINDER — your earlier plan was: \"" + pendingPlan + "\"\n" +
			"The tool result you needed is in the messages above. Now emit a JSON tool_call " +
			"for the next tool, copying values from that result directly into arguments. " +
			"Do not reply with type=\"text\" unless the user's question is fully answered.");

		// Append the reminder at the end so it is the freshest instruction the model sees.
		return rewrittenMessages.Concat(new[] { reminder });
	}
}
