#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.FontNamedSizeService))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete]
	class FontNamedSizeService : IFontNamedSizeService
	{
		readonly double _fontScalingFactor = 1;
		public FontNamedSizeService()
		{
#if __MOBILE__
			// The standard accessibility size for a font is 17, we can get a
			// close approximation to the new Size by multiplying by this scale factor
			_fontScalingFactor = (double)UIFont.PreferredBody.PointSize / 17f;
#endif
		}

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			// We make these up anyway, so new sizes didn't really change
			// iOS docs say default button font size is 15, default label font size is 17 so we use those as the defaults.
			var scalingFactor = _fontScalingFactor;

			if (Application.Current?.On<PlatformConfiguration.iOS>().GetEnableAccessibilityScalingForNamedFontSizes() == false)
			{
				scalingFactor = 1;
			}

			switch (size)
			{
				//We multiply the fonts by the scale factor, and cast to an int, to make them whole numbers.
				case NamedSize.Default:
					return (int)((typeof(Button).IsAssignableFrom(targetElementType) ? 15 : 17) * scalingFactor);
				case NamedSize.Micro:
					return (int)(12 * scalingFactor);
				case NamedSize.Small:
					return (int)(14 * scalingFactor);
				case NamedSize.Medium:
					return (int)(17 * scalingFactor);
				case NamedSize.Large:
					return (int)(22 * scalingFactor);
#if __IOS__
				case NamedSize.Body:
					return (double)UIFont.PreferredBody.PointSize;
				case NamedSize.Caption:
					return (double)UIFont.PreferredCaption1.PointSize;
				case NamedSize.Header:
					return (double)UIFont.PreferredHeadline.PointSize;
				case NamedSize.Subtitle:
					return (double)UIFont.PreferredTitle2.PointSize;
				case NamedSize.Title:
					return (double)UIFont.PreferredTitle1.PointSize;
#else
				case NamedSize.Body:
					return 23;
				case NamedSize.Caption:
					return 18;
				case NamedSize.Header:
					return 23;
				case NamedSize.Subtitle:
					return 28;
				case NamedSize.Title:
					return 34;

#endif
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
		}
	}
}