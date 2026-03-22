using System.ComponentModel;

namespace Microsoft.Maui;

/// <summary>
/// Enumerates values that control the layout direction for views.
/// </summary>
/// <remarks>
/// The default value for an element without a parent is <see cref="FlowDirection.LeftToRight"/>, even on platforms with right-to-left device defaults.
/// To opt in to right-to-left layout, set the root element's <c>FlowDirection</c> property to <see cref="FlowDirection.RightToLeft"/>, or use <see cref="FlowDirection.MatchParent"/> to follow the device.
/// All elements with a parent default to <see cref="FlowDirection.MatchParent"/>.
/// </remarks>
public enum FlowDirection
{
	/// <summary>Indicates that the view's layout direction will match the parent view's layout direction.</summary>
	MatchParent = 0,
	/// <summary>Indicates that the view will be arranged left to right. This is the default when the view has no parent.</summary>
	LeftToRight = 1,
	/// <summary>Indicates that the view will be arranged right to left.</summary>
	RightToLeft = 2,
}
