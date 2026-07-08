using System;
using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.GTK.FontNamedSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					return 11;
				case NamedSize.Micro:
				case NamedSize.Caption:
					return 12;
				case NamedSize.Medium:
					return 17;
				case NamedSize.Large:
					return 22;
				case NamedSize.Small:
				case NamedSize.Body:
					return 14;
				case NamedSize.Header:
					return 46;
				case NamedSize.Subtitle:
					return 20;
				case NamedSize.Title:
					return 24;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
		}
	}
}