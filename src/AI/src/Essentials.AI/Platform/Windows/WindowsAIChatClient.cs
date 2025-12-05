using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IChatClient"/> implementation based on native Windows AI APIs
/// </summary>
public sealed class WindowsAIChatClient : IChatClient
{
	/// <summary>
	/// Lazily-initialized metadata describing the implementation.
	/// </summary>
	private ChatClientMetadata? _metadata;

	/// <summary>
	/// Initializes a new <see cref="WindowsAIChatClient"/> instance.
	/// </summary>
	public WindowsAIChatClient()
	{
	}

	/// <inheritdoc />
	public async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		yield break;
		throw new NotImplementedException();
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
				providerName: "windows-ai",
				defaultModelId: "phi-silica");
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
}
