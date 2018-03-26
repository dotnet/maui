using System.Text;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public static class FormattedStringExtensions
	{
		public static SpannableString ToAttributed(this FormattedString formattedString, Font defaultFont, Color defaultForegroundColor, TextView view)
		{
			if (formattedString == null)
				return null;

			var builder = new StringBuilder();
			foreach (Span span in formattedString.Spans)
			{
				var text = span.Text;
				if (text == null)
					continue;

				builder.Append(text);
			}

			var spannable = new SpannableString(builder.ToString());

			var c = 0;
			foreach (Span span in formattedString.Spans)
			{
				var text = span.Text;
				if (text == null)
					continue;

				int start = c;
				int end = start + text.Length;
				c = end;

				if (span.TextColor != Color.Default)
				{
					spannable.SetSpan(new ForegroundColorSpan(span.TextColor.ToAndroid()), start, end, SpanTypes.InclusiveExclusive);
				}
				else if (defaultForegroundColor != Color.Default)
				{
					spannable.SetSpan(new ForegroundColorSpan(defaultForegroundColor.ToAndroid()), start, end, SpanTypes.InclusiveExclusive);
				}

				if (span.BackgroundColor != Color.Default)
				{
					spannable.SetSpan(new BackgroundColorSpan(span.BackgroundColor.ToAndroid()), start, end, SpanTypes.InclusiveExclusive);
				}

				if (!span.IsDefault())
#pragma warning disable 618 // We will need to update this when .Font goes away
					spannable.SetSpan(new FontSpan(span.Font, view), start, end, SpanTypes.InclusiveInclusive);
#pragma warning restore 618
				else if (defaultFont != Font.Default)
					spannable.SetSpan(new FontSpan(defaultFont, view), start, end, SpanTypes.InclusiveInclusive);
			}
			return spannable;
		}

		class FontSpan : MetricAffectingSpan
		{
			public FontSpan(Font font, TextView view)
			{
				Font = font;
				TextView = view;
			}

			public Font Font { get; }

			public TextView TextView { get; }

			public override void UpdateDrawState(TextPaint tp)
			{
				Apply(tp);
			}

			public override void UpdateMeasureState(TextPaint p)
			{
				Apply(p);
			}

			void Apply(Paint paint)
			{
				paint.SetTypeface(Font.ToTypeface());
				float value = Font.ToScaledPixel();
				paint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, value, TextView.Resources.DisplayMetrics);
			}
		}
	}
}