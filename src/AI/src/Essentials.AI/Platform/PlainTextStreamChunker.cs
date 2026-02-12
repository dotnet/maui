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

		// Simple substring delta - emit only new characters.
		// If the response is shorter than expected (snapshot regression), treat it as a reset.
		string delta;
		if (completeResponse.Length > _lastResponse.Length)
		{
			delta = completeResponse.Substring(_lastResponse.Length);
		}
		else if (completeResponse != _lastResponse)
		{
			// Response changed but didn't grow - treat as a reset and emit the full response
			delta = completeResponse;
		}
		else
		{
			delta = string.Empty;
		}

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
