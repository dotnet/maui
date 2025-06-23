namespace Microsoft.Maui;

/// <summary>
/// Enumerates values that control how the <see cref="GridLength.Value"/> property is interpreted for row and column definitions.
/// </summary>
public enum GridUnitType
{
	/// <summary>
	/// Interpret the <see cref="GridLength.Value"/> property value as the number of device-specific units.
	/// </summary>
	Absolute,

	/// <summary>
	/// Interpret the <see cref="GridLength.Value"/> property value as a proportional weight, to be arranged after rows and columns with <see cref="GridUnitType.Absolute"/> or <see cref="GridUnitType.Auto"/>.
	/// </summary>
	/// <remarks>
	/// After all Absolute and Auto rows/columns are arranged, remaining Star rows/columns receive a fraction of the leftover space determined by dividing each <see cref="GridLength.Value"/> by the sum of all star values.
	/// </remarks>
	Star,

	/// <summary>
	/// Ignore the <see cref="GridLength.Value"/> property value and choose a size that fits the children of the row or column.
	/// </summary>
	Auto
}
