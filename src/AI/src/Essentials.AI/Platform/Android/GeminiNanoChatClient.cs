using System.Runtime.CompilerServices;
using Google.MLKit.GenAI.Prompt;
using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on Google MLKit GenAI (Gemini Nano)
/// </summary>
public sealed class GeminiNanoChatClient : IChatClient
{
	/// <summary>The provider name for this chat client.</summary>
	private const string ProviderName = "google";

	/// <summary>The default model identifier.</summary>
	private const string DefaultModelId = "gemini-nano";

	/// <summary>The underlying generative model instance.</summary>
	private readonly IGenerativeModel _model;

	/// <summary>Whether this instance owns the model and is responsible for disposing it.</summary>
	private readonly bool _ownsModel;

	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

	/// <summary>
	/// Initializes a new instance of the <see cref="GeminiNanoChatClient"/> class.
	/// </summary>
	/// <remarks>
	/// The client will use the singleton <see cref="IGenerativeModel"/> for all requests.
	/// </remarks>
	public GeminiNanoChatClient()
	{
		_model = Generation.Instance.Client;
		_ownsModel = false; // Singleton model, we don't own it
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GeminiNanoChatClient"/> class
	/// with the specified <see cref="IGenerativeModel"/>.
	/// </summary>
	/// <param name="model">The <see cref="IGenerativeModel"/> to use for chat interactions.</param>
	/// <remarks>
	/// When using this constructor, the client does not own the <see cref="IGenerativeModel"/>
	/// and will not dispose it. The caller is responsible for disposing the model.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
	public GeminiNanoChatClient(IGenerativeModel model)
	{
		ArgumentNullException.ThrowIfNull(model);
		_model = model;
		_ownsModel = false;
	}

	/// <inheritdoc />
	public async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var request = ConvertToGenerateContentRequest(chatMessages, options);

		var response = await _model.GenerateContentAsync(request, cancellationToken);

		var aiContents = ConvertToContents(response);

		return new ChatResponse
		{
			Messages = { new ChatMessage(ChatRole.Assistant, aiContents) },
			ModelId = options?.ModelId ?? _metadata?.DefaultModelId ?? DefaultModelId,
			FinishReason = ConvertFinishReason(response)
		};
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = ConvertToGenerateContentRequest(chatMessages, options);

		var responseId = Guid.NewGuid().ToString("N");

		await foreach (var response in _model.GenerateContentStreamAsync(request, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();

			var aiContents = ConvertToContents(response);

			yield return new ChatResponseUpdate
			{
				Contents = aiContents,
				ModelId = options?.ModelId ?? _metadata?.DefaultModelId ?? DefaultModelId,
				Role = ChatRole.Assistant,
				ResponseId = responseId,
				FinishReason = ConvertFinishReason(response)
			};
		}

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
		if (_ownsModel)
		{
			_model.Dispose();
		}
	}

	private static GenerateContentRequest ConvertToGenerateContentRequest(
		IEnumerable<ChatMessage> chatMessages,
		ChatOptions? options)
	{
		var (systemPrompt, history) = NormalizeChatMessages(chatMessages, options);
		var (textPart, imagePart) = ConvertToParts(history);

		// Build request based on whether we have an image
		var builder = imagePart is not null
			? new GenerateContentRequest.Builder(imagePart, textPart)
			: new GenerateContentRequest.Builder(textPart);

		if (!string.IsNullOrEmpty(systemPrompt))
			builder.PromptPrefix = new PromptPrefix(systemPrompt);

		// Apply options
		if (options?.Temperature is float temperature)
			builder.Temperature = Java.Lang.Float.ValueOf(temperature);

		if (options?.TopK is int topK)
			builder.TopK = Java.Lang.Integer.ValueOf(topK);

		if (options?.Seed is long seed)
			builder.Seed = Java.Lang.Integer.ValueOf((int)seed);

		return builder.Build();
	}

	private static (TextPart TextPart, ImagePart? ImagePart) ConvertToParts(IEnumerable<ChatMessage> history)
	{
		ImagePart? imagePart = null;
		var textParts = new List<string>();

		foreach (var message in history)
		{
			foreach (var content in message.Contents)
			{
				if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
				{
					textParts.Add(textContent.Text);
				}
				else if (content is DataContent dataContent && imagePart == null)
				{
					// MLKit only supports one image per request (as shown in the Google sample)
					if (imagePart is not null)
					{
						throw new InvalidOperationException("Messages can only contain at most a single DataContent item.");
					}

					// Convert DataContent to Android Bitmap for ImagePart
					var bitmap = ConvertDataContentToBitmap(dataContent);
					if (bitmap != null)
					{
						imagePart = new ImagePart(bitmap);
					}
				}
				else
				{
					throw new ArgumentException($"Unsupported content type: {content.GetType().Name}", nameof(history));
				}
			}
		}

		var promptText = string.Join(Environment.NewLine, textParts);
		
		return (new TextPart(promptText), imagePart);
	}

	private static unsafe global::Android.Graphics.Bitmap? ConvertDataContentToBitmap(DataContent dataContent)
	{
		try
		{
			using var bytes = dataContent.Data.Pin();
			using var stream = new UnmanagedMemoryStream((byte*)bytes.Pointer, dataContent.Data.Length);
			return global::Android.Graphics.BitmapFactory.DecodeStream(stream);
		}
		catch
		{
			// If conversion fails, return null and skip the image
			return null;
		}
	}

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

	private static AIContent[] ConvertToContents(GenerateContentResponse response)
	{
		var candidates = response.Candidates;
		if (candidates == null || candidates.Count == 0)
			return [new TextContent(string.Empty)];

		var text = candidates[0]?.Text ?? string.Empty;
		return [new TextContent(text)];
	}

	private static ChatFinishReason? ConvertFinishReason(GenerateContentResponse response)
	{
		var candidates = response.Candidates;
		if (candidates == null || candidates.Count == 0)
			return null;

		return candidates[0]?.FinishReason switch
		{
			CandidateFinishReason.Stop => ChatFinishReason.Stop,
			CandidateFinishReason.MaxTokens => ChatFinishReason.Length,
			_ => null
		};
	}
}
