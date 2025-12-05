using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on native Apple Intelligence APIs
/// </summary>
[SupportedOSPlatform("ios26.0")]
[SupportedOSPlatform("maccatalyst26.0")]
[SupportedOSPlatform("macos26.0")]
public sealed class AppleIntelligenceChatClient : IChatClient
{
	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

	/// <summary>
	/// Initializes a new <see cref="AppleIntelligenceChatClient"/> instance.
	/// </summary>
	public AppleIntelligenceChatClient()
	{
	}

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
				if (node is JsonObject obj)
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
		Debug.WriteLine($"[AppleIntelligenceChatClient] GetResponseAsync called with {messages.Count()} messages.");

		var nativeMessages = messages.Select(ToNative).ToArray();
		var nativeOptions = options is null ? null : ToNative(options);
		var native = new ChatClientNative();

		var tcs = new TaskCompletionSource<ChatResponse>();

		var nativeToken = native.GetResponse(
			nativeMessages,
			nativeOptions,
			onComplete: (response, error) =>
			{
				if (error is not null)
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetResponseAsync encountered an error: {error.Domain} - {error.Code}");

					if (error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled)
					{
						tcs.TrySetCanceled();
					}
					else
					{
						tcs.TrySetException(new NSErrorException(error));
					}
					return;
				}

				Debug.WriteLine($"[AppleIntelligenceChatClient] GetResponseAsync completed successfully.");

				tcs.TrySetResult(FromNativeChatResponse(response));
			});

		cancellationToken.Register(() =>
		{
			Debug.WriteLine($"[AppleIntelligenceChatClient] GetResponseAsync cancellation requested.");

			nativeToken?.Cancel();
		});

		return tcs.Task;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync called with {messages.Count()} messages.");

		if (options?.ResponseFormat is ChatResponseFormatJson jsonFormat &&
			StrictSchemaTransformCache.GetOrCreateTransformedSchema(jsonFormat) is { } jsonSchema)
		{
			options.ResponseFormat = ChatResponseFormat.ForJsonSchema(
				jsonSchema, jsonFormat.SchemaName, jsonFormat.SchemaDescription);
		}

		// TODO: Handle ResponseFormat via system prompt
		// https://github.com/dotnet/maui/issues/32908
		var actualMessages = messages.ToList();
		// if (options?.ResponseFormat is ChatResponseFormatJson json)
		// {
		// 	actualMessages.Add(new ChatMessage(ChatRole.User,
		// 		$"""
		// 		ALWAYS format your response as a well-formed, complete JSON object according to
		// 		the specified schema:
		// 		{json.Schema}
		// 		"""));
		// }

		var nativeMessages = actualMessages.Select(ToNative).ToArray();
		var nativeOptions = options is null ? null : ToNative(options);

		// TODO: Handle ResponseFormat via system prompt
		// https://github.com/dotnet/maui/issues/32908
		// nativeOptions?.ResponseJsonSchema = null;

		var native = new ChatClientNative();

		var channel = Channel.CreateUnbounded<ChatResponseUpdate>();

		// var lastResponse = "";

		var nativeToken = native.StreamResponse(
			nativeMessages,
			nativeOptions,
			onUpdate: (update) =>
			{
				// Handle text updates
				if (update.Text is not null)
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync received text update: {update.Text}");

					// // Compute the partial update since Apple Intelligence returns the full response each time
					// var newResponse = update.Text;
					// var delta = newResponse.Substring(lastResponse.Length);
					// lastResponse = newResponse;

					// Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync computed delta: {delta}");

					// var chatUpdate = new ChatResponseUpdate
					// {
					// 	Role = ChatRole.Assistant,
					// 	Contents = { new TextContent(delta) }
					// };
					// channel.Writer.TryWrite(chatUpdate);
				}

				// Handle tool call notifications
				if (update.ToolCallName is not null && update.ToolCallId is not null && update.ToolCallArguments is not null)
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync received tool call: {update.ToolCallName} with arguments: {update.ToolCallArguments}");

#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
					var args = JsonSerializer.Deserialize<AIFunctionArguments>(update.ToolCallArguments, AIJsonUtilities.DefaultOptions);
#pragma warning restore IL3050, IL2026

					var chatUpdate = new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = { new FunctionCallContent(update.ToolCallId, update.ToolCallName, args) }
					};
					channel.Writer.TryWrite(chatUpdate);
				}

				// Handle tool result notifications
				if (update.ToolCallId is not null && update.ToolCallResult is not null)
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync received tool result for call ID: {update.ToolCallId}");

					var chatUpdate = new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = { new FunctionResultContent(update.ToolCallId, update.ToolCallResult) }
					};
					channel.Writer.TryWrite(chatUpdate);
				}
			},
			onComplete: (finalResult, error) =>
			{
				if (error is not null)
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync completed with error: {error.Domain} - {error.Code}");

					Exception ex = error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled
						? new OperationCanceledException()
						: new NSErrorException(error);

					channel.Writer.Complete(ex);
				}
				else
				{
					Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync completed successfully.");

					channel.Writer.Complete();
				}
			});

		cancellationToken.Register(() =>
		{
			Debug.WriteLine($"[AppleIntelligenceChatClient] GetStreamingResponseAsync cancellation requested.");

			nativeToken?.Cancel();
		});

		await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken))
		{
			yield return update;
		}
	}

	/// <inheritdoc />
	object? IChatClient.GetService(Type serviceType, object? serviceKey)
	{
		ArgumentNullException.ThrowIfNull(serviceType);

		// If there's a service key, we don't support keyed services.
		if (serviceKey is not null)
		{
			return null;
		}

		// If there's a request for metadata, lazily-initialize it and return it. We don't need to worry about race conditions,
		// as there's no requirement that the same instance be returned each time, and creation is idempotent.
		if (serviceType == typeof(ChatClientMetadata))
		{
			return _metadata ??= new ChatClientMetadata(
				providerName: "apple",
				defaultModelId: "apple-intelligence");
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
		// Nothing to dispose. Implementation required for the IChatClient interface.
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
			TextContent textContent when textContent.Text is not null => textContent.Text.Split("\n\n").Select(t => new TextContentNative(t)),
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
			Debug.WriteLine($"[AppleIntelligenceChatClient] Tool {Name} called with arguments: {arguments}");

			try
			{
				var aiArgs = JsonSerializer.Deserialize<AIFunctionArguments>(arguments, AIJsonUtilities.DefaultOptions);

				var result = await function.InvokeAsync(aiArgs, cancellationToken: default);

				var resultJson = result is not null
					? JsonSerializer.Serialize(result)
					: "{}";

				Debug.WriteLine($"[AppleIntelligenceChatClient] Tool {Name} returned result: {resultJson}");

				completion(new NSString(resultJson));
			}
			catch (Exception ex)
			{
				var errorJson = JsonSerializer.Serialize(new
				{
					error = ex.Message,
					type = ex.GetType().Name
				});

				Debug.WriteLine($"[AppleIntelligenceChatClient] Tool {Name} encountered an error: {errorJson}");

				completion(new NSString(errorJson));
			}

			Debug.WriteLine($"[AppleIntelligenceChatClient] Tool {Name} call completed.");
		}
#pragma warning restore IL3050, IL2026
	}
}
