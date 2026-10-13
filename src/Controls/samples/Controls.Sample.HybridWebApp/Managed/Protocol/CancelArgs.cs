namespace Maui.HybridApp;

/// <summary>Arguments for cooperative request cancellation.</summary>
/// <param name="RequestId">The active request UUID.</param>
public sealed record CancelArgs(string RequestId);
