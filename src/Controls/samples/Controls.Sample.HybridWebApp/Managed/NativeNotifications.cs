using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Maui.HybridApp;

/// <summary>Typed scoped OS notifications outside command operations, delivered only to the current subscribed document.</summary>
public sealed class NativeNotifications : IDisposable
{
	internal event Action<string, JsonElement, Func<bool>?>? Received;
	bool _disposed;


	/// <summary>Publishes provider-owned source-generated data, not arbitrary JavaScript or cross-window callbacks.</summary>
	/// <typeparam name="T">The provider's notification DTO.</typeparam>
	/// <param name="topic">An explicitly registered topic.</param>
	/// <param name="value">Small bounded notification data.</param>
	/// <param name="metadata">Strict generated metadata owned by the provider.</param>
	public void Publish<T>(string topic, T value, JsonTypeInfo<T> metadata)
		=> PublishCurrent(topic, value, metadata, null);

	internal void PublishCurrent<T>(string topic, T value, JsonTypeInfo<T> metadata, Func<bool>? isCurrent)
	{
		CommandRegistry.ValidateMetadata(metadata);
		if (!_disposed)
			Received?.Invoke(topic, JsonSerializer.SerializeToElement(value, metadata), isCurrent);
	}

	/// <summary>Removes listeners on scope disposal.</summary>
	public void Dispose() { _disposed = true; Received = null; }
}
