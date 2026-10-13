using System.Text.Json;

namespace Maui.HybridApp;

/// <summary>The session-scoped boundary passed to every command handler.</summary>
/// <param name="Session">The current document UUID.</param>
/// <param name="Id">The request UUID.</param>
/// <param name="Cancellation">Cooperative cancellation for deadline, cancel or document retirement.</param>
/// <param name="Publish">Publishes a provider-owned JSON payload to an explicitly registered topic.</param>
public sealed record Operation(string Session, string Id, CancellationToken Cancellation, Action<string, JsonElement> Publish);
