namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// A stream chunker for plain text responses that computes simple substring deltas.
/// </summary>
/// <remarks>
/// For plain text, the AI model outputs progressively longer text at each step.
/// We simply emit the new characters since the last response.
/// </remarks>
internal sealed class PlainTextStreamChunker : StreamChunkerBase
{
    /// <summary>Tracks the last complete response to compute deltas.</summary>
    private string _lastResponse = "";

    /// <inheritdoc />
    public override string Process(string completeResponse)
    {
        if (string.IsNullOrEmpty(completeResponse))
            return string.Empty;

        // Simple substring delta - emit only new characters
        var delta = completeResponse.Length > _lastResponse.Length
            ? completeResponse.Substring(_lastResponse.Length)
            : string.Empty;

        _lastResponse = completeResponse;
        return delta;
    }

    /// <inheritdoc />
    public override string Flush()
    {
        // Plain text has no pending state to flush
        return string.Empty;
    }
}
