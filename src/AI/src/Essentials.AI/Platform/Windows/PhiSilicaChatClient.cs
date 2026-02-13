using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.AI;
using Microsoft.Windows.AI.ContentSafety;
using Microsoft.Windows.AI.Text;
using Windows.Foundation;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on native Windows Copilot Runtime (Phi Silica)
/// </summary>
[SupportedOSPlatform("windows10.0.26100.0")]
public sealed class PhiSilicaChatClient : IChatClient
{
	/// <summary>The provider name for this chat client.</summary>
	private const string ProviderName = "windows";

	/// <summary>The default model identifier.</summary>
	private const string DefaultModelId = "phi-silica";

	/// <summary>Lazily-initialized task that creates the underlying <see cref="LanguageModel"/>.</summary>
	private Task<LanguageModel> _modelTask;

	/// <summary>Whether this instance owns the <see cref="LanguageModel"/> and is responsible for disposing it.</summary>
	private readonly bool _ownsModel;

	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

	/// <summary>
	/// Initializes a new instance of the <see cref="PhiSilicaChatClient"/> class.
	/// </summary>
	/// <remarks>
	/// The client will create a <see cref="LanguageModel"/> and reuse it for all requests.
	/// </remarks>
	public PhiSilicaChatClient()
	{
		_modelTask = PhiSilicaModelFactory.CreateModelAsync();
		_ownsModel = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PhiSilicaChatClient"/> class
	/// with the specified <see cref="LanguageModel"/>.
	/// </summary>
	/// <param name="model">The <see cref="LanguageModel"/> to use for chat interactions.</param>
	/// <remarks>
	/// When using this constructor, the client does not own the <see cref="LanguageModel"/>
	/// and will not dispose it. The caller is responsible for disposing the model.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
	public PhiSilicaChatClient(LanguageModel model)
	{
		ArgumentNullException.ThrowIfNull(model);
		_modelTask = Task.FromResult(model);
		_ownsModel = false;
	}

	/// <inheritdoc />
	public Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default) =>
		GetStreamingResponseAsync(chatMessages, options, cancellationToken).ToChatResponseAsync(cancellationToken: cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// Get the model (already created in constructor or provided)
		var model = await _modelTask;

		var (systemPrompt, history) = NormalizeChatMessages(chatMessages, options);

		using var context = string.IsNullOrEmpty(systemPrompt)
			? model.CreateContext()
			: model.CreateContext(systemPrompt, new ContentFilterOptions());

		var prompt = ConvertToPrompt(history);

		var modelOptions = ConvertToLanguageModelOptions(options);

		var responseId = Guid.NewGuid().ToString("N");

		// Use event-based API instead of await foreach
		var tcs = new TaskCompletionSource<string>();
		var fullResponse = new System.Text.StringBuilder();

		var operation = model.GenerateResponseAsync(context, prompt, modelOptions);

		operation.Progress = (operation, progress) =>
		{
			if (!string.IsNullOrEmpty(progress))
			{
				fullResponse.Append(progress);

				var aiContents = ConvertToAIContent(progress);

				var update = new ChatResponseUpdate
				{
					Contents = aiContents,
					ModelId = options?.ModelId ?? _metadata?.DefaultModelId ?? DefaultModelId,
					Role = ChatRole.Assistant,
					ResponseId = responseId
				};

				// Since we can't yield from a callback, we'll need a different approach
				// This is a limitation of the Windows AI API
			}
		};

		operation.Completed = (operation, status) =>
		{
			if (status == AsyncStatus.Completed)
			{
				tcs.TrySetResult(fullResponse.ToString());
			}
			else if (status == AsyncStatus.Error)
			{
				tcs.TrySetException(operation.ErrorCode);
			}
			else if (status == AsyncStatus.Canceled)
			{
				tcs.TrySetCanceled(cancellationToken);
			}
		};

		cancellationToken.Register(() => operation.Cancel());

		// Wait for completion
		var result = await tcs.Task;

		// Yield the full response as a single update
		yield return new ChatResponseUpdate
		{
			Contents = ConvertToAIContent(result),
			ModelId = options?.ModelId ?? _metadata?.DefaultModelId ?? DefaultModelId,
			Role = ChatRole.Assistant,
			ResponseId = responseId
		};

		// Final update to indicate completion
		yield return new ChatResponseUpdate
		{
			FinishReason = ChatFinishReason.Stop,
			ModelId = options?.ModelId ?? _metadata?.DefaultModelId ?? DefaultModelId,
			Role = ChatRole.Assistant,
			ResponseId = responseId
		};
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
		// If the task completed successfully, dispose the model
		if (_ownsModel && _modelTask.IsCompletedSuccessfully)
		{
			_modelTask.Result.Dispose();
		}
	}

	private static AIContent[] ConvertToAIContent(string response) =>
		[new TextContent(response ?? string.Empty)];

	private static (string SystemPrompt, List<ChatMessage> History) NormalizeChatMessages(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null)
	{
		var messages = chatMessages.ToList();

		// Use system instructions as the system prompt if provided
		if (options?.Instructions is { } system)
			return (system, messages);

		// Extract the first system message as the system prompt
		if (messages.Count > 0 && messages[0].Role == ChatRole.System)
		{
			var systemPrompt = messages[0].Text;
			messages.RemoveAt(0);

			return (systemPrompt, messages);
		}

		return (string.Empty, messages);
	}

	private static string ConvertToPrompt(IEnumerable<ChatMessage> history)
	{
		var promptParts = new List<string>();

		foreach (var message in history)
		{
			foreach (var content in message.Contents)
			{
				if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
				{
					promptParts.Add(textContent.Text);
				}
				else if (content is not TextContent)
				{
					throw new ArgumentException($"Unsupported content type: {content.GetType().Name}", nameof(history));
				}
			}
		}

		return string.Join(Environment.NewLine, promptParts);
	}

	private static LanguageModelOptions ConvertToLanguageModelOptions(ChatOptions? options)
	{
		if (options is null)
			return new();

		var languageModelOptions = new LanguageModelOptions();

		if (options.Temperature is { } temp)
			languageModelOptions.Temperature = temp;

		if (options.TopK is { } topK)
			languageModelOptions.TopK = (uint)topK;

		if (options.TopP is { } topP)
			languageModelOptions.TopP = (uint)topP;

		return languageModelOptions;
	}
}
