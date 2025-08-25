using System;

namespace Microsoft.Maui.Primitives;

/// <summary>
/// Determines if a view is constrained to a fixed size in either or both the horizontal and vertical directions.
/// </summary>
/// <remarks>
/// When a view is <see cref="Fixed"/> it will not propagate measure invalidation to its parent unless fixed size changes.
/// </remarks>
[Flags]
public enum SizeConstraint
{
	/// <summary>
	/// The size of the view is not fixed in either direction.
	/// </summary>
	None = 0,

	/// <summary>
	/// The size of the view is fixed in the horizontal direction.
	/// </summary>
	HorizontallyFixed = 1 << 0,

	/// <summary>
	/// The size of the view is fixed in the vertical direction.
	/// </summary>
	VerticallyFixed = 1 << 1,

	/// <summary>
	/// The size of the view is fixed in both directions.
	/// </summary>
	Fixed = HorizontallyFixed | VerticallyFixed
}