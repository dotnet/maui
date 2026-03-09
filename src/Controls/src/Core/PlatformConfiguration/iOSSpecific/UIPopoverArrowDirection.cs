using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	/// <summary>
	/// Enumerates valid popover arrow directions for iOS.
	/// </summary>
	[Flags]
	public enum UIPopoverArrowDirection
	{
		/// <summary>The arrow direction is unknown.</summary>
		Unknown = 0,
		/// <summary>The arrow points upward.</summary>
		Up = 1 << 0,
		/// <summary>The arrow points downward.</summary>
		Down = 1 << 1,
		/// <summary>The arrow points to the left.</summary>
		Left = 1 << 2,
		/// <summary>The arrow points to the right.</summary>
		Right = 1 << 3,
		/// <summary>The arrow can point in any direction.</summary>
		Any = Up | Down | Left | Right,
	}
}
