using System;

namespace Xamarin.Forms
{
	[Flags]
	public enum MeasureFlags
	{
		None = 0,
		IncludeMargins = 1 << 0
	}
}