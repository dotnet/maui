using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.FontNamedSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return size.GetFontSize();
		}
	}
}