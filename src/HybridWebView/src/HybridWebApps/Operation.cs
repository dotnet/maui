using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>The session-scoped boundary passed to every command handler.</summary>
/// <param name="Session">The current document UUID.</param>
/// <param name="Id">The request UUID.</param>
/// <param name="Cancellation">Cooperative cancellation for deadline, cancel or document retirement.</param>
/// <param name="Publish">Publishes a provider-owned JSON payload to an explicitly registered topic.</param>
public sealed record Operation(string Session, string Id, CancellationToken Cancellation, Action<string, JsonElement> Publish)
{
	internal byte[]? Payload { get; init; }
	internal Func<string, HybridChannelState>? ChannelFactory { get; init; }

	/// <summary>Gets the separate raw payload for a generated binary action; never reads JSON arrays.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public byte[] BinaryPayload => Payload ?? throw new CommandException("invalid_transport", "This action requires the binary transport.");

	/// <summary>Binds a document/operation-owned channel using compiler-generated payload metadata.</summary>
	/// <typeparam name="T">JSON contract or byte array sent by this action.</typeparam>
	/// <param name="id">The caller's scoped channel identity.</param>
	/// <param name="metadata">Generated JSON metadata, or null for raw byte messages.</param>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public HybridChannel<T> BindChannel<T>(string id, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>? metadata) =>
		new((ChannelFactory ?? throw new CommandException("invalid_transport", "Channels require a document hub."))(id), metadata);
}
