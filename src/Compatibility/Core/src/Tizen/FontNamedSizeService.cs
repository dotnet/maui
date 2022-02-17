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
					pt = Device.Idiom == TargetIdiom.TV || Device.Idiom == TargetIdiom.Watch ? 24 : 19;
					break;
				case NamedSize.Small:
					pt = Device.Idiom == TargetIdiom.TV ? 26 : (Device.Idiom == TargetIdiom.Watch ? 30 : 22);
					break;
				case NamedSize.Default:
				case NamedSize.Medium:
					pt = Device.Idiom == TargetIdiom.TV ? 28 : (Device.Idiom == TargetIdiom.Watch ? 32 : 25);
					break;
				case NamedSize.Large:
					pt = Device.Idiom == TargetIdiom.TV ? 32 : (Device.Idiom == TargetIdiom.Watch ? 36 : 31);
					break;
				case NamedSize.Body:
					pt = Device.Idiom == TargetIdiom.TV ? 30 : (Device.Idiom == TargetIdiom.Watch ? 32 : 28);
					break;
				case NamedSize.Caption:
					pt = Device.Idiom == TargetIdiom.TV ? 26 : (Device.Idiom == TargetIdiom.Watch ? 24 : 22);
					break;
				case NamedSize.Header:
					pt = Device.Idiom == TargetIdiom.TV ? 84 : (Device.Idiom == TargetIdiom.Watch ? 36 : 138);
					break;
				case NamedSize.Subtitle:
					pt = Device.Idiom == TargetIdiom.TV ? 30 : (Device.Idiom == TargetIdiom.Watch ? 30 : 28);
					break;
				case NamedSize.Title:
					pt = Device.Idiom == TargetIdiom.TV ? 42 : (Device.Idiom == TargetIdiom.Watch ? 36 : 40);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
			return Forms.ConvertToDPFont(pt);
		}
	}
}