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
		// Build a dynamic JSON schema for the tool call decision
		var schema = BuildToolCallSchema(tools);

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

		var systemPrompt = new ChatMessage(ChatRole.System,
			"You are a helpful assistant with access to tools.\n\n" +
			$"Available tools:\n{toolDescriptions}\n" +
			"Your response MUST be a JSON object matching the provided schema.\n" +
			"If the user's question requires a tool, set type to \"tool_call\", set tool_name, and provide arguments matching that tool's parameters.\n" +
			"If you can answer directly without a tool, set type to \"text\" and put your answer in the text field.\n" +
			"Call only ONE tool at a time. After receiving the result, you may call another.\n" +
			"For enum parameters, use EXACTLY one of the allowed values listed above.\n" +
			"If a tool has no required parameters, use an empty arguments object {}.\n" +
			"If you need information from one tool to call another, call the first tool and wait for its result.");

		// Clone options: remove tools, add structured output format
		var newOptions = options.Clone();
		newOptions.Tools = null;
		newOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema(
			schema,
			schemaName: "ToolCallDecision",
			schemaDescription: "Either a tool call or a text response");

		return ([systemPrompt, .. messages], newOptions);
	}

	private static JsonElement BuildToolCallSchema(List<AIFunction> tools)
	{
		// Build a simple discriminated union schema:
		// - type: "tool_call" or "text"  
		// - tool_name: one of the tool names (when type=tool_call)
		// - arguments: free-form object with tool parameters (when type=tool_call)
		// - text: response text (when type=text)
		//
		// NOTE: We intentionally keep "arguments" as a plain object without nested
		// property definitions. Merging different tools' schemas into one "arguments"
		// object confuses the model when tools have different parameter signatures.
		// The tool descriptions in the system prompt guide the model on what args to pass.
		var toolNames = tools.Select(t => $"\"{t.Name}\"").ToList();

		var schemaJson = $$"""
		{
			"type": "object",
			"properties": {
				"type": {
					"type": "string",
					"enum": ["tool_call", "text"],
					"description": "Whether this is a tool call or a text response"
				},
				"tool_name": {
					"type": "string",
					"enum": [{{string.Join(",", toolNames)}}],
					"description": "The name of the tool to call (only when type is tool_call)"
				},
				"arguments": {
					"type": "object",
					"description": "Arguments for the tool call matching the tool's parameter schema"
				},
				"text": {
					"type": "string",
					"description": "Your text response to the user (only when type is text)"
				}
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
				message.Contents.Add(new FunctionCallContent(
					callId: Guid.NewGuid().ToString("N")[..16],
					name: toolCall.ToolName!,
#pragma warning disable IL3050, IL2026
					arguments: toolCall.Arguments != null
						? JsonSerializer.Deserialize<Dictionary<string, object?>>(toolCall.Arguments.Value.GetRawText())
						: null));
#pragma warning restore IL3050, IL2026
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

				if (!string.IsNullOrEmpty(toolName))
					return new ToolCallDecision(toolName!, args);
			}
			else if (type == "text")
			{
				var textVal = root.TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
				return new TextResponseDecision(textVal ?? "");
			}
		}
		catch (JsonException)
		{
			// Not valid JSON
		}

		return null;
	}

	private record ToolCallDecision(string ToolName, JsonElement? Arguments);
	private record TextResponseDecision(string Text);
}
