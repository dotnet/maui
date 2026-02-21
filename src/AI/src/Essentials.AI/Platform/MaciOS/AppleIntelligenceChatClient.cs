using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on native Apple Intelligence APIs
/// </summary>
[SupportedOSPlatform("ios26.0")]
[SupportedOSPlatform("maccatalyst26.0")]
[SupportedOSPlatform("macos26.0")]
[SupportedOSPlatform("tvos26.0")]
public sealed class AppleIntelligenceChatClient : IChatClient
{
	/// <summary>The provider name for this chat client.</summary>
	private const string ProviderName = "apple";

	/// <summary>The default model identifier.</summary>
	private const string DefaultModelId = "apple-intelligence";

	// static AppleIntelligenceChatClient()
	// {
	// 	// Enable native logging for debugging purposes, this is quite verbose.
	// 	AppleIntelligenceLogger.Log = (message) => System.Diagnostics.Debug.WriteLine("[Native] " + message);
	// }

	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

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
	public Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var nativeMessages = ToNative(messages, options);
		var nativeOptions = ToNative(options, cancellationToken);
		var native = new ChatClientNative();

		var tcs = new TaskCompletionSource<ChatResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

		CancellationTokenRegistration registration = default;

		var nativeToken = native.GetResponse(
			nativeMessages,
			nativeOptions,
			onUpdate: (update) =>
			{
				// Updates are not used in non-streaming mode
			},
			onComplete: (response, error) =>
			{
				registration.Dispose();
				if (error is not null)
				{
					if (error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled)
					{
						tcs.TrySetCanceled();
					}
					else
					{
						tcs.TrySetException(new NSErrorException(error));
					}
				}
				else
				{
					var chatResponse = FromNativeChatResponse(response);
					tcs.TrySetResult(chatResponse);
				}
			});

		registration = cancellationToken.Register(() => nativeToken?.Cancel());

		return tcs.Task;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var nativeMessages = ToNative(messages, options);
		var nativeOptions = ToNative(options, cancellationToken);

		var native = new ChatClientNative();

		var channel = Channel.CreateUnbounded<ChatResponseUpdate>();

		// Use appropriate stream chunker based on response format
		StreamChunkerBase chunker = nativeOptions?.ResponseJsonSchema is not null
			? new JsonStreamChunker()
			: new PlainTextStreamChunker();

		CancellationTokenRegistration registration = default;

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

								channel.Writer.TryWrite(chatUpdate);
							}
						}
						break;

					case ResponseUpdateTypeNative.ToolCall:
						// Reset chunker so post-tool text is treated as a fresh stream
						chunker.Reset();

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
				registration.Dispose();
				if (error is not null)
				{
					if (error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled)
					{
						channel.Writer.Complete(new OperationCanceledException());
					}
					else
					{
						channel.Writer.Complete(new NSErrorException(error));
					}
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

						channel.Writer.TryWrite(finalUpdate);
					}

					channel.Writer.Complete();
				}
			});

		registration = cancellationToken.Register(() => nativeToken?.Cancel());

		await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken))
		{
			yield return update;
		}
	}

	/// <inheritdoc />
	object? IChatClient.GetService(Type serviceType, object? serviceKey)
	{
		ArgumentNullException.ThrowIfNull(serviceType);

		if (serviceKey is not null)
		{
			return null;
		}

		if (serviceType == typeof(ChatClientMetadata))
		{
			return _metadata ??= new ChatClientMetadata(
				providerName: ProviderName,
				defaultModelId: DefaultModelId);
		}

		if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return null;
	}

	/// <inheritdoc />
	void IDisposable.Dispose()
	{
		// Nothing to dispose.
	}

	private static ChatMessageNative[] ToNative(IEnumerable<ChatMessage> messages, ChatOptions? options)
	{
		ArgumentNullException.ThrowIfNull(messages);

		var toConvert = options?.Instructions is not null
			? messages.Prepend(new(ChatRole.System, options.Instructions))
			: messages;

		// Build a callId → name lookup from FunctionCallContent so FunctionResultContent can reference the tool name
		var callIdToName = new Dictionary<string, string>();
		foreach (var msg in messages)
		{
			foreach (var content in msg.Contents.OfType<FunctionCallContent>())
			{
				if (content.CallId is not null && content.Name is not null)
					callIdToName[content.CallId] = content.Name;
			}
		}

		// Filter out any messages that produce empty native content as a safety net.
		ChatMessageNative[] nativeMessages = [.. toConvert
			.Select(m => ToNative(m, callIdToName))
			.Where(m => m.Contents.Length > 0)];

		if (nativeMessages.Length == 0)
		{
			throw new ArgumentException("No messages with convertible content found. Ensure at least one message contains TextContent, FunctionCallContent, or FunctionResultContent.", nameof(messages));
		}

		return nativeMessages;
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

	private static ChatMessageNative ToNative(ChatMessage message, Dictionary<string, string>? callIdToName = null) =>
		new()
		{
			Role = ToNative(message.Role),
			Contents = [.. message.Contents.SelectMany(c => ToNative(c, callIdToName))]
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

	private static ChatOptionsNative? ToNative(ChatOptions? options, CancellationToken cancellationToken)
	{
		if (options is null)
		{
			return null;
		}

		if (options.MaxOutputTokens is <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(options), "The MaxOutputTokens option must be greater than zero.");
		}

		return new ChatOptionsNative
		{
			TopK = ToNative(options.TopK),
			Seed = ToNative(options.Seed),
			Temperature = ToNative(options.Temperature),
			MaxOutputTokens = ToNative(options.MaxOutputTokens),
			ResponseJsonSchema = ToNative(options.ResponseFormat),
			Tools = ToNative(options.Tools, cancellationToken)
		};
	}

	private static AIFunctionToolAdapter[]? ToNative(IList<AITool>? tools, CancellationToken cancellationToken)
	{
		AIFunctionToolAdapter[]? adapters = null;

		if (tools is { Count: > 0 })
		{
			var unsupportedTools = tools.Where(t => t is not AIFunction).ToList();
			if (unsupportedTools.Count > 0)
			{
				throw new NotSupportedException(
					$"Only AIFunction tools are supported by Apple Intelligence. " +
					$"Convert other tool types using AIFunctionFactory. " +
					$"Unsupported tools: {string.Join(", ", unsupportedTools.Select(t => t.GetType().Name))}");
			}

			adapters = tools
				.OfType<AIFunction>()
				.Select(function => new AIFunctionToolAdapter(function, cancellationToken))
				.ToArray();
		}

		return adapters;
	}

	private static NSString? ToNative(ChatResponseFormat? format) =>
		format switch
		{
			ChatResponseFormatJson jsonFormat when StrictSchemaTransformCache.GetOrCreateTransformedSchema(jsonFormat) is { } jsonSchema =>
				(NSString?)ChatResponseFormat.ForJsonSchema(jsonSchema, jsonFormat.SchemaName ?? "json_schema", jsonFormat.SchemaDescription).Schema.ToString(),
			ChatResponseFormatJson jsonFormat when jsonFormat.Schema is not null =>
				throw new InvalidOperationException("Failed to transform JSON schema for Apple Intelligence chat API."),
			ChatResponseFormatJson =>
				throw new InvalidOperationException("Apple Intelligence chat API requires a JSON schema for structured responses."),
			_ => null
		};

	private static IEnumerable<AIContentNative> ToNative(AIContent content, Dictionary<string, string>? callIdToName = null) =>
		content switch
		{
			// Apple Intelligence performs better when each text content chunk is separated
			TextContent textContent when textContent.Text is not null => [new TextContentNative(textContent.Text)],
			TextContent => Array.Empty<AIContentNative>(),

			// Function call/result content from prior tool-calling turns is converted to native types.
			// The native Swift layer gracefully skips these when building the Transcript, since Apple's
			// LanguageModelSession manages tool call state internally.
			FunctionCallContent functionCall => [new FunctionCallContentNative(
				functionCall.CallId ?? string.Empty,
				functionCall.Name,
#pragma warning disable IL3050, IL2026
				functionCall.Arguments is not null ? JsonSerializer.Serialize(functionCall.Arguments, AIJsonUtilities.DefaultOptions) : "{}")],
#pragma warning restore IL3050, IL2026

			FunctionResultContent functionResult => [new FunctionResultContentNative(
				functionResult.CallId ?? string.Empty,
				// Look up the tool name from the corresponding FunctionCallContent via callId
				(functionResult.CallId is not null && callIdToName?.TryGetValue(functionResult.CallId, out var name) == true) ? name : string.Empty,
#pragma warning disable IL3050, IL2026
				functionResult.Result is not null ? JsonSerializer.Serialize(functionResult.Result, AIJsonUtilities.DefaultOptions) : "{}")],
#pragma warning restore IL3050, IL2026

			// Throw for unsupported content types
			_ => throw new ArgumentException($"The content type '{content.GetType().FullName}' is not supported by Apple Intelligence chat APIs.", nameof(content))
		};

	private static NSNumber? ToNative(int? value) =>
		value.HasValue ? NSNumber.FromInt32(value.Value) : null;

	private static NSNumber? ToNative(double? value) =>
		value.HasValue ? NSNumber.FromDouble(value.Value) : null;

	private static NSNumber? ToNative(long? value) =>
		value.HasValue ? NSNumber.FromInt64(value.Value) : null;

	private sealed class AIFunctionToolAdapter(AIFunction function, CancellationToken cancellationToken) : AIToolNative
	{
		public override string Name => function.Name;

		public override string Desc => function.Description;

		public override string ArgumentsSchema => function.JsonSchema.GetRawText();

		public override string OutputSchema => function.ReturnJsonSchema?.GetRawText() ?? "{\"type\":\"string\"}";

#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
		public override async void CallWithArguments(NSString arguments, AIToolCompletionHandler completionHandler)
		{
			try
			{
				ArgumentNullException.ThrowIfNull(arguments);

				var aiArgs = JsonSerializer.Deserialize<AIFunctionArguments>((string)arguments, AIJsonUtilities.DefaultOptions);

				var result = await function.InvokeAsync(aiArgs, cancellationToken: cancellationToken);

				var resultJson = result is not null
					? JsonSerializer.Serialize(result)
					: "{}";

				completionHandler(new NSString(resultJson), null);
			}
			catch (OperationCanceledException)
			{
				var error = new NSError(new NSString(nameof(ChatClientNative)), (int)ChatClientError.Cancelled);

				completionHandler(null, error);
			}
			catch (Exception ex)
			{
				var userInfo = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
					[new NSString(ex.Message)],
					[NSError.LocalizedDescriptionKey]);

				var error = new NSError(new NSString("AIFunctionToolAdapterErrorDomain"), -1, userInfo);

				completionHandler(null, error);
			}
		}
#pragma warning restore IL3050, IL2026
	}
}
