namespace Microsoft.Maui.Controls;

/// <summary>
/// Enumeration specifying whether the ToolbarItem appears on the primary toolbar surface or secondary toolbar surface.
/// </summary>
public enum ToolbarItemOrder
{
	/// <summary>Use the default choice for the toolbar item.</summary>
	Default,

	/// <summary>Place the toolbar item on the primary toolbar surface.</summary>
	Primary,

	/// <summary>Place the toolbar item on the secondary toolbar surface.</summary>
	Secondary
}