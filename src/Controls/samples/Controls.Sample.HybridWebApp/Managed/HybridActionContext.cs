using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Maui.HybridApp;

/// <summary>The current operation boundary for an attributed action; never a cached document or root scope.</summary>
public sealed class HybridActionContext
{
	readonly Operation _operation;
	/// <summary>Creates the current operation boundary; generated handlers create a fresh context for every invocation.</summary>
	/// <param name="operation">The current host operation, never a cached document.</param>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public HybridActionContext(Operation operation) => _operation = operation ?? throw new ArgumentNullException(nameof(operation));
	/// <summary>Gets the current document session.</summary>
	public string Session => _operation.Session;
	/// <summary>Gets the current request identifier.</summary>
	public string Id => _operation.Id;
	/// <summary>Gets cancellation for this request and its owning document.</summary>
	public CancellationToken Cancellation => _operation.Cancellation;
	/// <summary>Publishes typed progress on a manually registered topic within the current operation.</summary>
	/// <typeparam name="T">The provider-owned payload contract.</typeparam>
	/// <param name="topic">An explicitly registered and subscribed topic.</param>
	/// <param name="value">The progress payload.</param>
	/// <param name="metadata">Strict source-generated JSON metadata owned by the provider.</param>
	public void Publish<T>(string topic, T value, JsonTypeInfo<T> metadata)
	{
		CommandRegistry.ValidateMetadata(metadata);
		Cancellation.ThrowIfCancellationRequested();
		_operation.Publish(topic, JsonSerializer.SerializeToElement(value, metadata));
	}
}
