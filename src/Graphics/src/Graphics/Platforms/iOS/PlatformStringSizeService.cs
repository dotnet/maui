using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public partial class PlatformStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			var nsString = new NSString(value);
			var uiFont = font?.ToPlatformFont(fontSize) ?? FontExtensions.GetDefaultPlatformFont();
			
			CGSize size;
			if (OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
			{
				size = nsString.GetBoundingRect(
					CGSize.Empty,
					NSStringDrawingOptions.UsesLineFragmentOrigin,
					new UIStringAttributes { Font = uiFont },
					null).Size;
			}
			else
			{
#pragma warning disable CA1422 // Validate platform compatibility
				size = nsString.StringSize(uiFont, CGSize.Empty);
#pragma warning restore CA1422 // Validate platform compatibility
			}

			uiFont.Dispose();
			return new SizeF((float)size.Width, (float)size.Height);
		}
	}
}
