namespace Maui.HybridApp;

/// <summary>Immutable host policy advertised by a scoped hybrid hub and used by its Essentials providers.</summary>
public sealed class HybridAppOptions
{
	/// <summary>Creates the host policy without selecting or enabling any provider.</summary>
	/// <param name="preferenceNamespace">The required, nonempty host-owned preference prefix.</param>
	/// <param name="maximumTextLength">The shared text limit in UTF-16 code units; defaults to 4096.</param>
	/// <param name="maximumWindows">Host-configured native window resource budget, including the initial window; defaults to 16.</param>
	public HybridAppOptions(string preferenceNamespace, int maximumTextLength = 4096, int maximumWindows = 16)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(preferenceNamespace);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumWindows);
		PreferenceNamespace = preferenceNamespace;
		MaximumTextLength = maximumTextLength;
		MaximumWindows = maximumWindows;
	}

	/// <summary>Gets the host-owned preference prefix; no sample namespace is supplied by the product.</summary>
	public string PreferenceNamespace { get; }

	/// <summary>Gets the text limit shared by the connection metadata and opted-in text providers.</summary>
	public int MaximumTextLength { get; }
	/// <summary>Gets the host-configured native window resource budget, not a universal app UX limit.</summary>
	public int MaximumWindows { get; }
}
