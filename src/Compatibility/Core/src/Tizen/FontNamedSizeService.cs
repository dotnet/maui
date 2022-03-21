using System;
using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Tizen.FontNamedSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			int pt;
			// Actual font size depends on the target idiom.
			switch (size)
			{
				case NamedSize.Micro:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV || DeviceInfo.Idiom == DeviceIdiom.Watch ? 24 : 19;
					break;
				case NamedSize.Small:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 26 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 30 : 22);
					break;
				case NamedSize.Default:
				case NamedSize.Medium:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 28 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 32 : 25);
					break;
				case NamedSize.Large:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 32 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 36 : 31);
					break;
				case NamedSize.Body:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 30 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 32 : 28);
					break;
				case NamedSize.Caption:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 26 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 24 : 22);
					break;
				case NamedSize.Header:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 84 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 36 : 138);
					break;
				case NamedSize.Subtitle:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 30 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 30 : 28);
					break;
				case NamedSize.Title:
					pt = DeviceInfo.Idiom == DeviceIdiom.TV ? 42 : (DeviceInfo.Idiom == DeviceIdiom.Watch ? 36 : 40);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
			return Forms.ConvertToDPFont(pt);
		}
	}
}