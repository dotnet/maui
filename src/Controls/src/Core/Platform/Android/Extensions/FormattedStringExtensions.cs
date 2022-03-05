#nullable enable
using System;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static SpannableString ToSpannableString(this Label label)
			=> ToSpannableString(label.FormattedText, label.RequireFontManager(), (label.Handler?.PlatformView as TextView)?.Paint, label.Handler?.MauiContext?.Context, label.LineHeight, label.HorizontalTextAlignment, label.ToFont(), label.TextColor, label.TextTransform);

		public static SpannableString ToSpannableString(this FormattedString formattedString, IFontManager fontManager, TextPaint? textPaint = null, Context? context = null, double defaultLineHeight = 0d, TextAlignment defaultHorizontalAlignment = TextAlignment.Start, Font? defaultFont = null, Graphics.Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default)
		{
			if (formattedString == null)
				return new SpannableString(string.Empty);

			var builder = new StringBuilder();
			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];

				var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;

				var text = TextTransformUtilites.GetTransformedText(span.Text, transform);
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

				var fgcolor = span.TextColor ?? defaultColor;
				if (fgcolor != null)
				{
					spannable.SetSpan(new ForegroundColorSpan(fgcolor.ToPlatform()), start, end, SpanTypes.InclusiveExclusive);
				}

				if (span.BackgroundColor != null)
				{
					spannable.SetSpan(new BackgroundColorSpan(span.BackgroundColor.ToPlatform()), start, end, SpanTypes.InclusiveExclusive);
				}

				var lineHeight = span.LineHeight >= 0
					? span.LineHeight
					: defaultLineHeight;

				if (lineHeight >= 0)
				{
					spannable.SetSpan(new LineHeightSpan(textPaint, lineHeight), start, end, SpanTypes.InclusiveExclusive);
				}

				var font = span.ToFont();
				if (font.IsDefault && defaultFont.HasValue)
					font = defaultFont.Value;

				if (!font.IsDefault)
				{
					spannable.SetSpan(
						new FontSpan(font, context, span.CharacterSpacing.ToEm(), defaultHorizontalAlignment, fontManager),
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
			public FontSpan(Font font, Context? context, float characterSpacing, TextAlignment? horizontalTextAlignment, IFontManager fontManager)
			{
				Font = font;
				Context = context;
				CharacterSpacing = characterSpacing;
				FontManager = fontManager;
				HorizontalTextAlignment = horizontalTextAlignment;
			}

			public readonly IFontManager FontManager;
			public readonly Font Font;
			public readonly Context? Context;
			public readonly float CharacterSpacing;
			public readonly TextAlignment? HorizontalTextAlignment;

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
					value, Context?.Resources?.DisplayMetrics ?? Android.App.Application.Context.Resources!.DisplayMetrics);

				paint.LetterSpacing = CharacterSpacing;

				if (HorizontalTextAlignment.HasValue)
				{
					paint.TextAlign = HorizontalTextAlignment.Value switch
					{
						TextAlignment.Start => Paint.Align.Left,
						TextAlignment.Center => Paint.Align.Center,
						TextAlignment.End => Paint.Align.Right,
						_ => Paint.Align.Left
					};
				}
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

			public LineHeightSpan(TextPaint? paint, double lineHeight)
			{
				_lineHeight = lineHeight;
				var fm = paint?.GetFontMetricsInt();
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