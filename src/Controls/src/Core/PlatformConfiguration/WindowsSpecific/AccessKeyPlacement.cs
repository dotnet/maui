using System;

namespace Microsoft.Maui.Controls
{

	/// <summary>Enumerates access key placement relative to the control that the access key describes.</summary>
	public enum AccessKeyPlacement
	{
		/// <summary>Indicates that the default access key placement will be used.</summary>
		Auto = 0,
		/// <summary>Indicates that the access key will appear above the top edge of the described element.</summary>
		Top,
		/// <summary>Indicates that the access key will appear below the lower edge of the described element.</summary>
		Bottom,
		/// <summary>Indicates that the access key will appear to the right of the right edge of the described element.</summary>
		Right,
		/// <summary>Indicates that the access key will appear to the left of the left edge of the described element.</summary>
		Left,
		/// <summary>Indicates that the access key will be overlaid on top of the center of the described element.</summary>
		Center,
	}
}