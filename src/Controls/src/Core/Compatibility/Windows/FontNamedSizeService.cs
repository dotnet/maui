using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.FontNamedSizeService))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete]
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return size.GetFontSize();
		}
	}
}