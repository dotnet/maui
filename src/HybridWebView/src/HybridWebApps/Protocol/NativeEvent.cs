using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>A session-scoped native event envelope.</summary>
/// <param name="Version">The protocol version.</param>
/// <param name="Session">The document UUID.</param>
/// <param name="Id">The originating request UUID.</param>
/// <param name="Topic">An explicitly registered event topic.</param>
/// <param name="Data">The provider's serialized payload.</param>
public sealed record NativeEvent(int Version, string Session, string Id, string Topic, JsonElement Data);
