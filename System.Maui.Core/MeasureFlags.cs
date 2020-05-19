using System;

namespace System.Maui
{
	[Flags]
	public enum MeasureFlags
	{
		None = 0,
		IncludeMargins = 1 << 0
	}
}