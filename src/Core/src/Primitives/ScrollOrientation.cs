namespace Microsoft.Maui;

/// <summary>
/// Enumeration specifying vertical or horizontal scrolling directions.
/// </summary>
public enum ScrollOrientation
{
	/// <summary>The content scrolls vertically.</summary>
	Vertical = 0,

	/// <summary>The content scrolls horizontally.</summary>
	Horizontal,

	/// <summary>The content can scroll both horizontally and vertically.</summary>
	Both,

	/// <summary>The content cannot scroll.</summary>
	Neither
}
