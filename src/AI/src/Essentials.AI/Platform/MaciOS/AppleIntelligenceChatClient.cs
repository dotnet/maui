using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on native Apple Intelligence APIs
/// </summary>
[SupportedOSPlatform("ios26.0")]
[SupportedOSPlatform("maccatalyst26.0")]
[SupportedOSPlatform("macos26.0")]
public sealed partial class AppleIntelligenceChatClient : ChatClientBase
{
#if DEBUG
	static AppleIntelligenceChatClient()
	{
		// Enable native logging for debugging purposes, this is quite verbose.
		// AppleIntelligenceLogger.Log = (message) => System.Diagnostics.Debug.WriteLine("[Native] " + message);
	}
#endif


	/// <summary>
	/// Initializes a new <see cref="AppleIntelligenceChatClient"/> instance.
	/// </summary>
	public AppleIntelligenceChatClient()
		: this(null)
	{
	}

	/// <summary>
	/// Initializes a new <see cref="AppleIntelligenceChatClient"/> instance with the specified logger.
	/// </summary>
	/// <param name="logger">An optional <see cref="ILogger"/> instance for logging chat operations.</param>
	public AppleIntelligenceChatClient(ILogger? logger)
		: base(logger)
	{
	}

	/// <summary>
	/// Initializes a new <see cref="AppleIntelligenceChatClient"/> instance with the specified logger.
	/// </summary>
	/// <param name="logger">An optional <see cref="ILogger"/> instance for logging chat operations.</param>
	public AppleIntelligenceChatClient(ILogger<AppleIntelligenceChatClient>? logger)
		: base(logger)
	{
	}

	/// <inheritdoc />
	internal override string ProviderName => "apple";

	/// <inheritdoc />
	internal override string DefaultModelId => "apple-intelligence";

	internal static AIJsonSchemaTransformCache StrictSchemaTransformCache { get; } =
		new(new()
		{
			DisallowAdditionalProperties = true,
			ConvertBooleanSchemas = true,
			MoveDefaultKeywordToDescription = true,
			RequireAllProperties = true,
			TransformSchemaNode = (ctx, node) =>
			{
				// Handle objects
				if (node is JsonObject obj && obj.TryGetPropertyValue("type", out var typeNode) && typeNode?.GetValue<string>() == "object")
				{
					// All objects need a title
					if (!obj.ContainsKey("title"))
					{
						obj["title"] = Guid.NewGuid().ToString("N");
					}
				}

				return node;
			},
		});

	/// <inheritdoc />
	public override Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		LogMethodInvoked(nameof(GetResponseAsync), messages, options);

		var nativeMessages = messages.Select(ToNative).ToArray();
		var nativeOptions = options is null ? null : ToNative(options);
		var native = new ChatClientNative();

		var tcs = new TaskCompletionSource<ChatResponse>();

		var nativeToken = native.GetResponse(
			nativeMessages,
			nativeOptions,
			onUpdate: (update) =>
			{
				switch (update.UpdateType)
				{
					case ResponseUpdateTypeNative.ToolCall:
						LogFunctionInvoking(nameof(GetResponseAsync), update.ToolCallName!, update.ToolCallId!, update.ToolCallArguments);
						break;

					case ResponseUpdateTypeNative.ToolResult:
						LogFunctionInvocationCompleted(nameof(GetResponseAsync), update.ToolCallId!, update.ToolCallResult!);
						break;

					case ResponseUpdateTypeNative.Content:
					default:
						// Content updates are not used in non-streaming mode
						break;
				}
			},
			onComplete: (response, error) =>
			{
				if (error is not null)
				{
					if (error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled)
					{
						LogMethodCanceled(nameof(GetResponseAsync));
						tcs.TrySetCanceled();
					}
					else
					{
						var ex = new NSErrorException(error);
						LogMethodFailed(nameof(GetResponseAsync), ex);
						tcs.TrySetException(ex);
					}

					return;
				}

				var chatResponse = FromNativeChatResponse(response);
				LogMethodCompleted(nameof(GetResponseAsync), chatResponse);
				tcs.TrySetResult(chatResponse);
			});

		cancellationToken.Register(() => nativeToken?.Cancel());

		return tcs.Task;
	}

	/// <inheritdoc />
	public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		LogMethodInvoked(nameof(GetStreamingResponseAsync), messages, options);

		if (options?.ResponseFormat is ChatResponseFormatJson jsonFormat &&
			StrictSchemaTransformCache.GetOrCreateTransformedSchema(jsonFormat) is { } jsonSchema)
		{
			options.ResponseFormat = ChatResponseFormat.ForJsonSchema(
				jsonSchema, jsonFormat.SchemaName, jsonFormat.SchemaDescription);
		}

		var nativeMessages = messages.Select(ToNative).ToArray();
		var nativeOptions = options is null ? null : ToNative(options);

		var native = new ChatClientNative();

		var channel = Channel.CreateUnbounded<ChatResponseUpdate>();

		// Use appropriate stream chunker based on response format
		StreamChunkerBase chunker = options?.ResponseFormat is ChatResponseFormatJson
			? new JsonStreamChunker()
			: new PlainTextStreamChunker();

		var nativeToken = native.StreamResponse(
			nativeMessages,
			nativeOptions,
			onUpdate: (update) =>
			{
				switch (update.UpdateType)
				{
					case ResponseUpdateTypeNative.Content:
						// Handle text updates
						if (update.Text is not null)
						{
							// Use stream chunker to compute delta - handles both JSON and plain text
							var delta = chunker.Process(update.Text);

							if (!string.IsNullOrEmpty(delta))
							{
								var chatUpdate = new ChatResponseUpdate
								{
									Role = ChatRole.Assistant,
									Contents = { new TextContent(delta) }
								};

								LogStreamingUpdate(nameof(GetStreamingResponseAsync), chatUpdate);
								channel.Writer.TryWrite(chatUpdate);
							}
						}
						break;

					case ResponseUpdateTypeNative.ToolCall:
						LogFunctionInvoking(nameof(GetStreamingResponseAsync), update.ToolCallName!, update.ToolCallId!, update.ToolCallArguments);

						var args = update.ToolCallArguments is null
							? null
#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
							: JsonSerializer.Deserialize<AIFunctionArguments>(update.ToolCallArguments, AIJsonUtilities.DefaultOptions);
#pragma warning restore IL3050, IL2026

						var toolCallUpdate = new ChatResponseUpdate
						{
							Role = ChatRole.Assistant,
							Contents = { new FunctionCallContent(update.ToolCallId!, update.ToolCallName!, args) }
						};
						channel.Writer.TryWrite(toolCallUpdate);
						break;

					case ResponseUpdateTypeNative.ToolResult:
						LogFunctionInvocationCompleted(nameof(GetStreamingResponseAsync), update.ToolCallId!, update.ToolCallResult!);

						var toolResultUpdate = new ChatResponseUpdate
						{
							Role = ChatRole.Assistant,
							Contents = { new FunctionResultContent(update.ToolCallId!, update.ToolCallResult!) }
						};
						channel.Writer.TryWrite(toolResultUpdate);
						break;
				}
			},
			onComplete: (finalResult, error) =>
			{
				if (error is not null)
				{
					Exception ex;
					if (error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled)
					{
						LogMethodCanceled(nameof(GetStreamingResponseAsync));
						ex = new OperationCanceledException();
					}
					else
					{
						ex = new NSErrorException(error);
						LogMethodFailed(nameof(GetStreamingResponseAsync), ex);
					}

					channel.Writer.Complete(ex);
				}
				else
				{
					// Flush any remaining content from the chunker
					var finalChunk = chunker.Flush();
					if (!string.IsNullOrEmpty(finalChunk))
					{
						var finalUpdate = new ChatResponseUpdate
						{
							Role = ChatRole.Assistant,
							Contents = { new TextContent(finalChunk) }
						};

						LogStreamingUpdate(nameof(GetStreamingResponseAsync), finalUpdate);
						channel.Writer.TryWrite(finalUpdate);
					}

					var chatResponse = FromNativeChatResponse(finalResult);
					LogMethodCompleted(nameof(GetStreamingResponseAsync), chatResponse);
					channel.Writer.Complete();
				}
			});

		cancellationToken.Register(() => nativeToken?.Cancel());

		await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken))
		{
			yield return update;
		}
	}

	private static ChatResponse FromNativeChatResponse(ChatResponseNative? response)
	{
		if (response is null || response.Messages is null || response.Messages.Length == 0)
		{
			// Fallback: return empty response
			return new ChatResponse([new ChatMessage(ChatRole.Assistant, "")]);
		}

		// Convert all native messages to ChatMessage objects
		var messages = response.Messages
			.Select(FromNative)
			.ToList();

		// Create ChatResponse with all messages
		return new ChatResponse(messages);
	}

	private static ChatMessage FromNative(ChatMessageNative nativeMessage)
	{
		var message = new ChatMessage
		{
			Role = FromNative(nativeMessage.Role)
		};

		if (nativeMessage.Contents is not null)
		{
			foreach (var content in nativeMessage.Contents)
			{
				message.Contents.Add(FromNative(content));
			}
		}

		return message;
	}

	private static ChatRole FromNative(ChatRoleNative role) =>
		role switch
		{
			ChatRoleNative.User => ChatRole.User,
			ChatRoleNative.Assistant => ChatRole.Assistant,
			ChatRoleNative.System => ChatRole.System,
			ChatRoleNative.Tool => ChatRole.Tool,
			_ => throw new ArgumentOutOfRangeException(nameof(role), $"Unknown role: {role}")
		};

	private static AIContent FromNative(AIContentNative content) =>
		content switch
		{
			TextContentNative textContent =>
				new TextContent(textContent.Text),

			FunctionCallContentNative functionCall =>
#pragma warning disable IL3050, IL2026
				new FunctionCallContent(
					functionCall.CallId,
					functionCall.Name,
					JsonSerializer.Deserialize<AIFunctionArguments>(
						functionCall.Arguments,
						AIJsonUtilities.DefaultOptions)),
#pragma warning restore IL3050, IL2026

			FunctionResultContentNative functionResult =>
				new FunctionResultContent(
					functionResult.CallId,
					functionResult.Result),

			_ => throw new ArgumentException($"Unsupported content type: {content.GetType().Name}", nameof(content))
		};

	private static ChatMessageNative ToNative(ChatMessage message) =>
		new()
		{
			Role = ToNative(message.Role),
			Contents = [.. message.Contents.SelectMany(ToNative)]
		};

	private static ChatRoleNative ToNative(ChatRole role)
	{
		if (role == ChatRole.User)
			return ChatRoleNative.User;
		else if (role == ChatRole.Assistant)
			return ChatRoleNative.Assistant;
		else if (role == ChatRole.System)
			return ChatRoleNative.System;
		else if (role == ChatRole.Tool)
			return ChatRoleNative.Tool;
		else
			throw new ArgumentOutOfRangeException(nameof(role), $"The role '{role}' is not supported by Apple Intelligence chat APIs.");
	}

	private static ChatOptionsNative ToNative(ChatOptions options) =>
		new ChatOptionsNative
		{
			TopK = ToNative(options.TopK),
			Seed = ToNative(options.Seed),
			Temperature = ToNative(options.Temperature),
			MaxOutputTokens = ToNative(options.MaxOutputTokens),
			ResponseJsonSchema = ToNative(options.ResponseFormat),
			Tools = ToNative(options.Tools)
		};

	private static AIFunctionToolAdapter[]? ToNative(IList<AITool>? tools)
	{
		AIFunctionToolAdapter[]? adapters = null;

		if (tools is { Count: > 0 })
		{
			// Note: Only AIFunction tools are supported for Apple Intelligence
			// Other AITool implementations should be converted to AIFunction using AIFunctionFactory

			adapters = tools
				.OfType<AIFunction>()
				.Select(function => new AIFunctionToolAdapter(function))
				.ToArray();
		}

		return adapters;
	}

	private static NSString? ToNative(ChatResponseFormat? format) =>
		format switch
		{
			ChatResponseFormatJson jsonFormat => (NSString?)jsonFormat.Schema.ToString(),
			_ => null
		};

	private static IEnumerable<AIContentNative> ToNative(AIContent content) =>
		content switch
		{
			// Apple Intelligence performs better when each text content chunk is separated
			TextContent textContent when textContent.Text is not null => [new TextContentNative(textContent.Text)],
			TextContent => Array.Empty<AIContentNative>(),

			// Throw for unsupported content types
			_ => throw new ArgumentException($"The content type '{content.GetType().FullName}' is not supported by Apple Intelligence chat APIs.", nameof(content))
		};

	private static NSNumber? ToNative(int? value) =>
		value.HasValue ? NSNumber.FromInt32(value.Value) : null;

	private static NSNumber? ToNative(double? value) =>
		value.HasValue ? NSNumber.FromDouble(value.Value) : null;

	private static NSNumber? ToNative(long? value) =>
		value.HasValue ? NSNumber.FromInt64(value.Value) : null;

	private sealed class AIFunctionToolAdapter(AIFunction function) : AIToolNative
	{
		public override string Name => function.Name;

		public override string Desc => function.Description;

		public override string ArgumentsSchema => function.JsonSchema.GetRawText();

		public override string OutputSchema => function.ReturnJsonSchema?.GetRawText() ?? "{\"type\":\"string\"}";

#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
		public override async void CallWithArguments(NSString arguments, Action<NSString> completion)
		{
			try
			{
				var aiArgs = JsonSerializer.Deserialize<AIFunctionArguments>(arguments, AIJsonUtilities.DefaultOptions);

				var result = await function.InvokeAsync(aiArgs, cancellationToken: default);

				var resultJson = result is not null
					? JsonSerializer.Serialize(result)
					: "{}";

				completion(new NSString(resultJson));
			}
			catch (Exception ex)
			{
				var errorJson = JsonSerializer.Serialize(new
				{
					error = ex.Message,
					type = ex.GetType().Name
				});

				completion(new NSString(errorJson));
			}
		}
#pragma warning restore IL3050, IL2026
	}
}
