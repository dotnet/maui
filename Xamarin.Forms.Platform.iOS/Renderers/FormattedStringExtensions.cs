using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public static class FormattedStringExtensions
	{
		public static NSAttributedString ToAttributed(this Span span, Font defaultFont, Color defaultForegroundColor)
		{
			if (span == null)
				return null;

#pragma warning disable 0618 //retaining legacy call to obsolete code
			var font = span.Font != Font.Default ? span.Font : defaultFont;
#pragma warning restore 0618
			var fgcolor = span.ForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = defaultForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = Color.Black; // as defined by apple docs		

			return new NSAttributedString(span.Text, font == Font.Default ? null : font.ToUIFont(), fgcolor.ToUIColor(), span.BackgroundColor.ToUIColor());
		}

		public static NSAttributedString ToAttributed(this FormattedString formattedString, Font defaultFont, Color defaultForegroundColor)
		{
			if (formattedString == null)
				return null;
			var attributed = new NSMutableAttributedString();
			foreach (var span in formattedString.Spans)
			{
				if (span.Text == null)
					continue;

				attributed.Append(span.ToAttributed(defaultFont, defaultForegroundColor));
			}

			return attributed;
		}

		internal static NSAttributedString ToAttributed(this Span span, Element owner, Color defaultForegroundColor)
		{
			if (span == null)
				return null;

			UIFont targetFont;
			if (span.IsDefault())
				targetFont = ((IFontElement)owner).ToUIFont();
			else
				targetFont = span.ToUIFont();

			var fgcolor = span.ForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = defaultForegroundColor;
			if (fgcolor.IsDefault)
				fgcolor = Color.Black; // as defined by apple docs

			return new NSAttributedString(span.Text, targetFont, fgcolor.ToUIColor(), span.BackgroundColor.ToUIColor());
		}

		internal static NSAttributedString ToAttributed(this FormattedString formattedString, Element owner, Color defaultForegroundColor)
		{
			if (formattedString == null)
				return null;
			var attributed = new NSMutableAttributedString();
			foreach (var span in formattedString.Spans)
			{
				if (span.Text == null)
					continue;

				attributed.Append(span.ToAttributed(owner, defaultForegroundColor));
			}

			return attributed;
		}
	}
}