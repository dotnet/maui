namespace Microsoft.Maui;

/// <summary>
/// Provides functionality to provide a border.
/// </summary>
public interface IBorder
{
	/// <summary>
	///  Define how the Shape outline is painted.
	/// </summary>
	IBorderStroke Border { get; }
}
