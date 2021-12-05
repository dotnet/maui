#nullable enable
using System;
using System.Text;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static SpannableString ToSpannable(this FormattedString formattedString, Label label, TextView view)
		{
			if (formattedString == null)
				return new SpannableString(string.Empty);

			IFontManager? fontManager = default;

			var builder = new StringBuilder();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				var text = TextTransformUtilites.GetTransformedText(span.Text, span.TextTransform);
				if (text == null)
					continue;

				builder.Append(text);
			}

			var spannable = new SpannableString(builder.ToString());

			var c = 0;
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				var text = span.Text;
				if (text == null)
					continue;

				int start = c;
				int end = start + text.Length;
				c = end;

				var fgcolor = span.TextColor ?? label.TextColor;
				if (fgcolor != null)
				{
					spannable.SetSpan(new ForegroundColorSpan(fgcolor.ToNative()), start, end, SpanTypes.InclusiveExclusive);
				}

				if (span.BackgroundColor != null)
				{
					spannable.SetSpan(new BackgroundColorSpan(span.BackgroundColor.ToNative()), start, end, SpanTypes.InclusiveExclusive);
				}
				if (span.LineHeight >= 0)
				{
					spannable.SetSpan(new LineHeightSpan(view, span.LineHeight), start, end, SpanTypes.InclusiveExclusive);
				}

				var font = span.ToFont();
				if (font.IsDefault)
					font = label.ToFont();

				if (!font.IsDefault)
				{
					spannable.SetSpan(
						new FontSpan(font, view, span.CharacterSpacing.ToEm(), fontManager ??= label.Handler.GetRequiredService<IFontManager>()),
						start,
						end,
						SpanTypes.InclusiveInclusive);
				}

				if (span.IsSet(Span.TextDecorationsProperty))
				{
					spannable.SetSpan(new TextDecorationSpan(span), start, end, SpanTypes.InclusiveInclusive);
				}

			}
			return spannable;
		}

		class FontSpan : MetricAffectingSpan
		{
			public FontSpan(Font font, TextView view, float characterSpacing, IFontManager fontManager)
			{
				Font = font;
				TextView = view;
				CharacterSpacing = characterSpacing;
				FontManager = fontManager;
			}

			public readonly IFontManager FontManager;
			public readonly Font Font;
			public readonly TextView TextView;
			public readonly float CharacterSpacing;

			public override void UpdateDrawState(TextPaint? tp)
			{
				if (tp != null)
					Apply(tp);
			}

			public override void UpdateMeasureState(TextPaint p)
			{
				Apply(p);
			}

			void Apply(Paint paint)
			{
				paint.SetTypeface(Font.ToTypeface(FontManager));
				float value = (float)Font.Size;

				paint.TextSize = TypedValue.ApplyDimension(
					Font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip,
					value, TextView?.Resources?.DisplayMetrics ?? Android.App.Application.Context.Resources!.DisplayMetrics);

				paint.LetterSpacing = CharacterSpacing;
			}
		}

		class TextDecorationSpan : MetricAffectingSpan
		{
			public TextDecorationSpan(Span span)
			{
				Span = span;
			}

			public Span Span { get; }

			public override void UpdateDrawState(TextPaint? tp)
			{
				if (tp != null)
					Apply(tp);
			}

			public override void UpdateMeasureState(TextPaint p)
			{
				Apply(p);
			}

			void Apply(Paint paint)
			{
				var textDecorations = Span.TextDecorations;
				paint.UnderlineText = (textDecorations & TextDecorations.Underline) != 0;
				paint.StrikeThruText = (textDecorations & TextDecorations.Strikethrough) != 0;
			}
		}

		class LineHeightSpan : Java.Lang.Object, ILineHeightSpan
		{
			private double _lineHeight;
			private int _ascent;
			private int _descent;

			public LineHeightSpan(TextView view, double lineHeight)
			{
				_lineHeight = lineHeight;
				var fm = view.Paint?.GetFontMetricsInt();
				_ascent = fm?.Ascent ?? 1;
				_descent = fm?.Descent ?? 1;
			}

			public void ChooseHeight(Java.Lang.ICharSequence? text, int start, int end, int spanstartv, int v, Paint.FontMetricsInt? fm)
			{
				if (fm != null)
				{
					fm.Ascent = (int)(_ascent * _lineHeight);
					fm.Descent = (int)(_descent * _lineHeight);
				}
			}
		}
	}
}