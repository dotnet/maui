using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Platform.Text
{
	internal static class AttributedTextExtensions
	{
		public static SpannableStringBuilder AsSpannableStringBuilder(this IAttributedText target)
		{
			if (target != null)
			{
				var spannableString = new SpannableStringBuilder(target.Text);
				HandleRuns(target, spannableString);
				return spannableString;
			}

			return null;
		}

		public static SpannableString AsSpannableString(this IAttributedText target)
		{
			if (target != null)
			{
				var spannableString = new SpannableString(target.Text);
				HandleRuns(target, spannableString);
				return spannableString;
			}

			return null;
		}

		private static void HandleRuns(
			IAttributedText target,
			ISpannable spannableString)
		{
			foreach (var run in target.Runs)
				HandleFormatRun(spannableString, run);
		}

		private static void HandleFormatRun(ISpannable spannableString, IAttributedTextRun run)
		{
			var attributes = run.Attributes;
			var start = run.Start;
			var end = start + run.Length;

			var fontName = attributes.GetFontName();
			if (fontName != null)
			{
				var typefaceSpan = new TypefaceSpan(fontName);
				spannableString.SetSpan(typefaceSpan, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetBold() && attributes.GetItalic())
			{
				var span = new StyleSpan(TypefaceStyle.BoldItalic);
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}
			else if (attributes.GetBold())
			{
				var span = new StyleSpan(TypefaceStyle.Bold);
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}
			else if (attributes.GetItalic())
			{
				var span = new StyleSpan(TypefaceStyle.Italic);
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetUnderline())
			{
				var span = new UnderlineSpan();
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			var foregroundColor = attributes.GetForegroundColor()?.ParseAsInts()?.ToColor();
			if (foregroundColor != null)
			{
				var span = new ForegroundColorSpan((global::Android.Graphics.Color)foregroundColor);
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			var backgroundColor = attributes.GetBackgroundColor()?.ParseAsInts()?.ToColor();
			if (backgroundColor != null)
			{
				var span = new BackgroundColorSpan((global::Android.Graphics.Color)backgroundColor);
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetSubscript())
			{
				var span = new SubscriptSpan();
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetSuperscript())
			{
				var span = new SuperscriptSpan();
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetStrikethrough())
			{
				var span = new StrikethroughSpan();
				spannableString.SetSpan(span, start, end, SpanTypes.ExclusiveExclusive);
			}

			if (attributes.GetUnorderedList())
			{
				var bulletSpan = new BulletSpan();
				spannableString.SetSpan(bulletSpan, start, end, SpanTypes.ExclusiveExclusive);
			}
		}
	}
}
