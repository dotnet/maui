using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates values that tell whether margins are included when laying out windows.</summary>
	[Flags]
	public enum MeasureFlags
	{
		/// <summary>Do not include margins in a layout measurement.</summary>
		None = 0,
		/// <summary>Include margins in a layout measurement.</summary>
		IncludeMargins = 1 << 0
	}
}