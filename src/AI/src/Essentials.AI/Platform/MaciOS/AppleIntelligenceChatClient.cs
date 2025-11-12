using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Channels;
using Microsoft.Extensions.AI;

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

	/// <inheritdoc />
	public Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var nativeMessages = chatMessages.Select(ToNative).ToArray();

		var nativeOptions = options is null ? null : ToNative(options);

		var native = new ChatClientNative();

		var tcs = new TaskCompletionSource<ChatResponse>();

		var nativeToken = native.GetResponse(nativeMessages, nativeOptions, (response, error) =>
		{
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
				
				return;
			}

			tcs.TrySetResult(FromNativeChatResponse(response));
		});

		cancellationToken.Register(() =>
		{
			nativeToken?.Cancel();
		});

		return tcs.Task;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// TODO: Handle ResponseFormat via system prompt
		// https://github.com/dotnet/maui/issues/32908
		var actualMessages = chatMessages.ToList();
		if (options?.ResponseFormat is ChatResponseFormatJson json)
		{
			actualMessages.Add(new ChatMessage(ChatRole.User,
				$"""
				Please format your response as a JSON object according to the specified schema.
				Ensure that the JSON is well-formed and complete.
				
				IMPORTANT: DO NOT wrap with triple backticks! ONLY return the JSON object!

				{json.Schema}
				"""));
		}

		var nativeMessages = actualMessages.Select(ToNative).ToArray();
		var nativeOptions = options is null ? null : ToNative(options);
		nativeOptions?.ResponseJsonSchema = null; // TODO: Handled via system prompt above
		var native = new ChatClientNative();

		var channel = Channel.CreateUnbounded<ChatResponseUpdate>();

		var lastResponse = "";

		var nativeToken = native.StreamResponse(
			nativeMessages,
			nativeOptions,
			onUpdate: (update) =>
			{
				if (update is null)
					return;

				// Compute the partial update since Apple Intelligence returns the full response each time
				var newResponse = update.ToString() ?? "";
				var delta = newResponse.Substring(lastResponse.Length);
				lastResponse = newResponse;

				var chatUpdate = new ChatResponseUpdate
				{
					Role = ChatRole.Assistant,
					Contents =
                    {
                        new TextContent(delta)
                    }
				};
				
				channel.Writer.TryWrite(chatUpdate);
			},
			onComplete: (finalResult, error) =>
			{
				if (error is null)
				{
					channel.Writer.Complete();
				}
				else
				{
					Exception ex = error.Domain == nameof(ChatClientNative) && error.Code == (int)ChatClientError.Cancelled
						? new OperationCanceledException()
						: new NSErrorException(error);

					channel.Writer.Complete(ex);
				}
			});

		cancellationToken.Register(() =>
		{
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

	private static ChatResponse FromNativeChatResponse(NSString? response) =>
		new (new ChatMessage(
			ChatRole.Assistant,
			response?.ToString()));

	private static ChatMessageNative ToNative(ChatMessage message) => 
		new()
		{
			Role = ToNative(message.Role),
			Contents = [.. message.Contents.Select(ToNative)]
		};

	private static ChatRoleNative ToNative(ChatRole role)
	{
		if (role == ChatRole.User)
			return ChatRoleNative.User;
		else if (role == ChatRole.Assistant)
			return ChatRoleNative.Assistant;
		else if (role == ChatRole.System)
			return ChatRoleNative.System;
		else
			throw new ArgumentOutOfRangeException(nameof(role), $"The role '{role}' is not supported by Apple Intelligence chat APIs.");
	}

	private static ChatOptionsNative ToNative(ChatOptions options) =>
		new()
		{
			TopK = ToNative(options.TopK),
			Seed = ToNative(options.Seed),
			Temperature = ToNative(options.Temperature),
			MaxOutputTokens = ToNative(options.MaxOutputTokens),
			ResponseJsonSchema = ToNative(options.ResponseFormat)
		};

	private static NSString? ToNative(ChatResponseFormat? format) =>
		format switch
		{
			ChatResponseFormatJson jsonFormat => (NSString?)jsonFormat.Schema.ToString(),
			_ => null
		};

	private static AIContentNative ToNative(AIContent content) =>
		content switch
		{
			TextContent textContent => new TextContentNative(textContent.Text),
			_ => throw new ArgumentException($"The content type '{content.GetType().FullName}' is not supported by Apple Intelligence chat APIs.", nameof(content))
		};

	private static NSNumber? ToNative(int? value) =>
		value.HasValue ? NSNumber.FromInt32(value.Value) : null;

	private static NSNumber? ToNative(double? value) =>
		value.HasValue ? NSNumber.FromDouble(value.Value) : null;

	private static NSNumber? ToNative(long? value) =>
		value.HasValue ? NSNumber.FromInt64(value.Value) : null;
}
