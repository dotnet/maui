using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates flags that indicate whether the layout direction was explicitly set, and whether the layout direction is right-to-left.</summary>
	[Flags]
	public enum EffectiveFlowDirection
	{
		/// <summary>Indicates that the flow direction is right-to-left.</summary>
		RightToLeft = 1 << 0,
		/// <summary>Indicates that the developer explicitly set the flow direction.</summary>
		Explicit = 1 << 1
	}
}