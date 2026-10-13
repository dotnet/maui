namespace Maui.HybridApp;

/// <summary>Immutable host policy advertised by a scoped hybrid hub and used by its Essentials providers.</summary>
public sealed class HybridAppOptions
{
	/// <summary>Creates the host policy without selecting or enabling any provider.</summary>
	/// <param name="preferenceNamespace">The required, nonempty host-owned preference prefix.</param>
	/// <param name="maximumTextLength">The shared text limit in UTF-16 code units; defaults to 4096.</param>
	public HybridAppOptions(string preferenceNamespace, int maximumTextLength = 4096)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(preferenceNamespace);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTextLength);
		PreferenceNamespace = preferenceNamespace;
		MaximumTextLength = maximumTextLength;
	}

	/// <summary>Gets the host-owned preference prefix; no sample namespace is supplied by the product.</summary>
	public string PreferenceNamespace { get; }

	/// <summary>Gets the text limit shared by the connection metadata and opted-in text providers.</summary>
	public int MaximumTextLength { get; }
}
