namespace Microsoft.Maui.Performance;

/// <summary>
/// Defines performance tracking categories.
/// </summary>
internal enum PerformanceCategory : byte
{
	/// <summary>
	/// Represents the layout measurement phase.
	/// </summary>
	LayoutMeasure,

	/// <summary>
	/// Represents the layout arrangement phase.
	/// </summary>
	LayoutArrange,
}