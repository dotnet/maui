namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Base class for stream chunkers that convert complete responses back into streaming chunks.
/// </summary>
/// <remarks>
/// Used when an AI model outputs complete responses at each step, but we want to stream
/// partial output to the user for better UX. Implementations compare successive snapshots
/// and emit only the delta (new/changed content).
/// </remarks>
internal abstract class StreamChunkerBase
{
    /// <summary>
    /// Processes a complete snapshot and returns a streaming chunk representing the delta.
    /// </summary>
    /// <param name="completeResponse">A complete response representing the current state.</param>
    /// <returns>A string chunk to emit. Concatenating all chunks yields the final response.</returns>
    public abstract string Process(string completeResponse);

    /// <summary>
    /// Flushes any remaining state and closes all pending output.
    /// </summary>
    /// <returns>Final chunk to complete the output (may be empty).</returns>
    public abstract string Flush();
}
