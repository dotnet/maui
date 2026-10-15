namespace Maui.HybridApp;

/// <summary>Immutable host policy advertised by a scoped hybrid hub and used by its Essentials providers.</summary>
public sealed class HybridAppOptions
{
	/// <summary>Creates the host policy without selecting or enabling any provider.</summary>
	/// <param name="maximumTextLength">The clipboard/share text limit in UTF-16 code units; defaults to 4096.</param>
	/// <param name="maximumWindows">Host-configured native window resource budget, including the initial window; defaults to 16.</param>
	public HybridAppOptions(int maximumTextLength = 4096, int maximumWindows = 16)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumWindows);
		MaximumTextLength = maximumTextLength;
		MaximumWindows = maximumWindows;
	}

	/// <summary>Gets the clipboard/share text limit advertised by the connection metadata, not a speech or preference limit.</summary>
	public int MaximumTextLength { get; }
	/// <summary>Gets the host-configured native window resource budget, not a universal app UX limit.</summary>
	public int MaximumWindows { get; }
}
