using System.Text.Json;

namespace Maui.HybridApp;

/// <summary>A protocol-1 terminal response envelope.</summary>
/// <param name="Version">The protocol version.</param>
/// <param name="Session">The document UUID.</param>
/// <param name="Id">The request UUID.</param>
/// <param name="Status">Success, error or cancelled.</param>
/// <param name="Result">The provider's result, or null on failure.</param>
/// <param name="Error">A deliberate error, or null on success.</param>
public sealed record Response(int Version, string Session, string Id, string Status, JsonElement? Result, CommandError? Error);
