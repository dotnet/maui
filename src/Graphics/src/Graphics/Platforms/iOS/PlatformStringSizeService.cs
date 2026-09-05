using System;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	public partial class PlatformStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value))
			{
				return new SizeF();
			}

			// ToCTFont creates a new CTFont instance that we own and must dispose.
			// GetDefaultCTFont may return a shared instance, so we don't dispose it.
			using var ownedFont = font?.ToCTFont(fontSize);

			var attributes = new CTStringAttributes
			{
				Font = ownedFont ?? FontExtensions.GetDefaultCTFont(fontSize)
			};

			// Get suggested frame size with unlimited constraints
			using var attributedString = new NSAttributedString(value, attributes);
			using var framesetter = new CTFramesetter(attributedString);

			var measuredSize = framesetter.SuggestFrameSize(
				new NSRange(0, attributedString.Length),
				null,
				new CGSize(float.MaxValue, float.MaxValue),
				out _);

			return new SizeF((float)measuredSize.Width, (float)measuredSize.Height);
		}
	}
}