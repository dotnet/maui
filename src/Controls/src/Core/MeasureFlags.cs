using System;

namespace Microsoft.Maui.Controls
{
	[Flags]
	public enum MeasureFlags
	{
		None = 0,
		IncludeMargins = 1 << 0
	}
}