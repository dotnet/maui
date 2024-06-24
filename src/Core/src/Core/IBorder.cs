namespace Microsoft.Maui;

/// <summary>
/// Provides functionality to provide a border.
/// </summary>
[Obsolete("IBorder is not used and will be removed in a future release. Use IBorderView or IBorderStroke instead.")]
public interface IBorder
{
	/// <summary>
	///  Define how the Shape outline is painted.
	/// </summary>
	IBorderStroke Border { get; }
}
