#nullable enable
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Controls.Internals;
using AAplication = Android.App.Application;

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

			var defaultFontSize = defaultFont?.Size ?? fontManager.DefaultFontSize;

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

				var textColor = span.TextColor ?? defaultColor;

				if (textColor != null)
				{
					spannable.SetSpan(new ForegroundColorSpan(textColor.ToPlatform()), start, end, SpanTypes.InclusiveExclusive);
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

				var font = span.ToFont(defaultFontSize);
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

		public static void RecalculateSpanPositions(this TextView textView, Label element, SpannableString spannableString, SizeRequest finalSize)
		{
			if (element?.FormattedText?.Spans == null || element.FormattedText.Spans.Count == 0)
				return;

			var labelWidth = finalSize.Request.Width;

			if (labelWidth <= 0 || finalSize.Request.Height <= 0)
				return;

			if (spannableString == null || spannableString.IsDisposed())
				return;

			var layout = textView.Layout;

			if (layout == null)
				return;

			int next = 0;
			int count = 0;
			IList<int> totalLineHeights = new List<int>();

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
			for (int i = 0; i < spannableString.Length(); i = next)
			{
				var type = Java.Lang.Class.FromType(typeof(Java.Lang.Object));

				var span = element.FormattedText.Spans[count];

				count++;

				if (string.IsNullOrEmpty(span.Text))
					continue;

				// Find the next span
				next = spannableString.NextSpanTransition(i, spannableString.Length(), type);

				// Get all spans in the range - Android can have overlapping spans				
				var spans = spannableString.GetSpans(i, next, type);

				if (spans == null)
					continue;

				var startSpan = spans[0];
				var endSpan = spans[spans.Length - 1];

				var startSpanOffset = spannableString.GetSpanStart(startSpan);
				var endSpanOffset = spannableString.GetSpanEnd(endSpan);

				var thisLine = layout.GetLineForOffset(endSpanOffset);
				var lineStart = layout.GetLineStart(thisLine);
				var lineEnd = layout.GetLineEnd(thisLine);

				// If this is true, endSpanOffset has the value for another line that belong to the next span and not it self. 
				// So it should be rearranged to value not pass the lineEnd.
				if (endSpanOffset > (lineEnd - lineStart))
					endSpanOffset = lineEnd;

				var startX = layout.GetPrimaryHorizontal(startSpanOffset);
				var endX = layout.GetPrimaryHorizontal(endSpanOffset);

				var startLine = layout.GetLineForOffset(startSpanOffset);
				var endLine = layout.GetLineForOffset(endSpanOffset);

				double[] lineHeights = new double[endLine - startLine + 1];

				// Calculate all the different line heights
				for (var lineCount = startLine; lineCount <= endLine; lineCount++)
				{
					var lineHeight = layout.GetLineBottom(lineCount) - layout.GetLineTop(lineCount);
					lineHeights[lineCount - startLine] = lineHeight;

					if (totalLineHeights.Count <= lineCount)
						totalLineHeights.Add(lineHeight);
				}

				var yaxis = 0.0;

				for (var line = startLine; line > 0; line--)
					yaxis += totalLineHeights[line];

				((ISpatialElement)span).Region = Region.FromLines(lineHeights, labelWidth, startX, endX, yaxis).Inflate(10);
			}
#pragma warning restore CA1416 // 'SpannableString.Length()' is only supported on: 'android' 29.0 and later
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
					value, Context?.Resources?.DisplayMetrics ?? AAplication.Context.Resources!.DisplayMetrics);

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