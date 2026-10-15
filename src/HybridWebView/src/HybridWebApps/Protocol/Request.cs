using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>A protocol-1 request envelope.</summary>
/// <param name="Version">The protocol version.</param>
/// <param name="Session">The document UUID.</param>
/// <param name="Id">The request UUID.</param>
/// <param name="Command">The explicitly registered command.</param>
/// <param name="Args">Arguments validated using the provider's generated metadata.</param>
public sealed record Request(int Version, string Session, string Id, string Command, JsonElement Args);
