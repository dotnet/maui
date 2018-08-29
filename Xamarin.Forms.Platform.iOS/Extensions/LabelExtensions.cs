using CoreGraphics;
using Foundation;
using System.Collections.Generic;
using System.Drawing;
using Xamarin.Forms.Internals;
#if __MOBILE__
using UIKit;
using NativeLabel = UIKit.UILabel;
#else
using AppKit;
using NativeLabel = AppKit.NSTextField;
#endif

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
	internal static class LabelExtensions
	{
		public static void RecalculateSpanPositions(this NativeLabel control, Label element)
		{
			if (element?.FormattedText?.Spans == null
				|| element.FormattedText.Spans.Count == 0)
				return;

			var finalSize = control.Frame;

			if (finalSize.Width <= 0 || finalSize.Height <= 0)
				return;

#if __MOBILE__
			var inline = control.AttributedText;
#else
			var inline = control.AttributedStringValue;
#endif
			var range = new NSRange(0, inline.Length);

			NSTextStorage textStorage = new NSTextStorage();
			textStorage.SetString(inline);

			var layoutManager = new NSLayoutManager();
			textStorage.AddLayoutManager(layoutManager);

			var textContainer = new NSTextContainer(size: finalSize.Size)
			{
				LineFragmentPadding = 0
			};

			layoutManager.AddTextContainer(textContainer);

			var labelWidth = finalSize.Width;

			var currentLocation = 0;

			for (int i = 0; i < element.FormattedText.Spans.Count; i++)
			{
				var span = element.FormattedText.Spans[i];

				var location = currentLocation;
				var length = span.Text?.Length ?? 0;

				if (length == 0)
					continue;

				var startRect = GetCharacterBounds(new NSRange(location, 1), layoutManager, textContainer);
				var endRect = GetCharacterBounds(new NSRange(location + length, 1), layoutManager, textContainer);

				var startLineHeight = startRect.Bottom - startRect.Top;
				var endLineHeight = endRect.Bottom - endRect.Top;

				var defaultLineHeight = control.FindDefaultLineHeight(location, length);

				var yaxis = startRect.Top;
				var lineHeights = new List<double>();

				while ((endRect.Bottom - yaxis) > 0.001)
				{
					double lineHeight;
					if (yaxis == startRect.Top) // First Line
					{
						lineHeight = startRect.Bottom - startRect.Top;
					}
					else if (yaxis != endRect.Top) // Middle Line(s)
					{
						lineHeight = defaultLineHeight;
					}
					else // Bottom Line
					{
						lineHeight = endRect.Bottom - endRect.Top;
					}
					lineHeights.Add(lineHeight);
					yaxis += (float)lineHeight;
				}

				((ISpatialElement)span).Region = Region.FromLines(lineHeights.ToArray(), finalSize.Width, startRect.X, endRect.X, startRect.Top).Inflate(10);

				// update current location
				currentLocation += length;
			}
		}

		static CGRect GetCharacterBounds(NSRange characterRange, NSLayoutManager layoutManager, NSTextContainer textContainer)
		{
			var glyphRange = new NSRange();

#if __MOBILE__
			layoutManager.CharacterRangeForGlyphRange(characterRange, ref glyphRange);
#else
			layoutManager.CharacterRangeForGlyphRange(characterRange, out glyphRange);
#endif
			return layoutManager.BoundingRectForGlyphRange(glyphRange, textContainer);
		}

		static double FindDefaultLineHeight(this NativeLabel control, int start, int length)
		{
			if (length == 0)
				return 0.0;

			var textStorage = new NSTextStorage();
#if __MOBILE__
			textStorage.SetString(control.AttributedText.Substring(start, length));
#else
			textStorage.SetString(control.AttributedStringValue.Substring(start, length));
#endif
			var layoutManager = new NSLayoutManager();
			textStorage.AddLayoutManager(layoutManager);

			var textContainer = new NSTextContainer(size: new SizeF(float.MaxValue, float.MaxValue))
			{
				LineFragmentPadding = 0
			};
			layoutManager.AddTextContainer(textContainer);

			var rect = GetCharacterBounds(new NSRange(0, 1), layoutManager, textContainer);
			return rect.Bottom - rect.Top;
		}

	}
}
