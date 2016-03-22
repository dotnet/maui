using System;

namespace Xamarin.Forms.Platform.Android
{
	[Flags]
	internal enum MeasureSpecificationType
	{
		Unspecified = 0,
		Exactly = 0x1 << 31,
		AtMost = 0x1 << 32,
		Mask = Exactly | AtMost
	}
}