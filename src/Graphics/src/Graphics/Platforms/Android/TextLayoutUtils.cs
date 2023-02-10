using System.Drawing;
using Android.Text;

namespace Microsoft.Maui.Graphics.Platform
{
	internal static class TextLayoutUtils
	{
		public static StaticLayout CreateLayout(string text, TextPaint textPaint, int? boundedWidth, Layout.Alignment alignment)
		{
			int finalWidth = int.MaxValue;
			if (boundedWidth > 0)
				finalWidth = (int)boundedWidth;

#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
			var layout = new StaticLayout(
				text, // Text to layout
				textPaint, // Text paint (font, size, etc...) to use
				finalWidth, // The maximum width the text can be
				alignment, // The horizontal alignment of the text
				1.0f, // Spacing multiplier
				0.0f, // Additional spacing
				false); // Include padding
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility

			return layout;
		}

		public static StaticLayout CreateLayoutForSpannedString(SpannableString spannedString, TextPaint textPaint, int? boundedWidth, Layout.Alignment alignment)
		{
			int finalWidth = int.MaxValue;
			if (boundedWidth > 0)
				finalWidth = (int)boundedWidth;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
			var layout = new StaticLayout(
				spannedString, // Text to layout
				textPaint, // Text paint (font, size, etc...) to use
				finalWidth, // The maximum width the text can be
				alignment, // The horizontal alignment of the text
				1.0f, // Spacing multiplier
				0.0f, // Additional spacing
				false); // Include padding
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS0618 // Type or member is obsolete

			return layout;
		}

		public static SizeF GetTextSize(this StaticLayout target)
		{
			// Get the text bounds and assume (the safe assumption) that the layout wasn't
			// created with a bounded width.
			return GetTextSize(target, false);
		}

		public static SizeF GetTextSize(this StaticLayout target, bool hasBoundedWidth)
		{
			// We need to know if the static layout was created with a bounded width, as this is what
			// StaticLayout.Width returns.
			if (hasBoundedWidth)
				return new SizeF(target.Width, target.Height);

			float vMaxWidth = 0;
			int vLineCount = target.LineCount;

			for (int i = 0; i < vLineCount; i++)
			{
				float vLineWidth = target.GetLineWidth(i);
				if (vLineWidth > vMaxWidth)
					vMaxWidth = vLineWidth;
			}

			return new SizeF(vMaxWidth, target.Height);
		}
	}
}
