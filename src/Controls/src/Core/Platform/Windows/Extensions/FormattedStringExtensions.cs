using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		static double GetMeasuredLineHeight(DependencyObject obj) =>
			(double)obj.GetValue(MeasuredLineHeightProperty);

		static void SetMeasuredLineHeight(DependencyObject obj, double value) =>
			obj.SetValue(MeasuredLineHeightProperty, value);

		static readonly DependencyProperty MeasuredLineHeightProperty = DependencyProperty.RegisterAttached(
			"MeasuredLineHeight", typeof(double), typeof(FormattedStringExtensions), new PropertyMetadata(0));

		public static void UpdateInlines(this TextBlock textBlock, Label label)
			=> UpdateInlines(
				textBlock,
				label.RequireFontManager(false),
				label.FormattedText,
				label.LineHeight,
				label.HorizontalTextAlignment,
				label.ToFont(),
				label.TextColor,
				label.TextTransform);

		public static void UpdateInlines(
			this TextBlock textBlock,
			IFontManager fontManager,
			FormattedString formattedString,
			double defaultLineHeight = 0d, // TODO: NET8 should be -1, but too late to change for net6
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
		{
			textBlock.Inlines.Clear();

			// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
			textBlock.Measure(new global::Windows.Foundation.Size(double.MaxValue, double.MaxValue));

			var runs = formattedString.ToRunAndColorsTuples(
				fontManager,
				defaultLineHeight,
				defaultHorizontalAlignment,
				defaultFont,
				defaultColor,
				defaultTextTransform).ToArray();

			var lineHeights = new List<double>(runs.Length);
			foreach (var (run, _, _) in runs)
			{
				lineHeights.Add(textBlock.FindDefaultLineHeight(run));
			}

			var currentTextIndex = 0;
			for (var i = 0; i < runs.Length; i++)
			{
				var (run, textColor, background) = runs[i];
				var runTextLength = run.Text.Length;

				SetMeasuredLineHeight(run, lineHeights[i]);

				textBlock.Inlines.Add(run);

				if (background is not null || textColor is not null)
				{
					var textHighlighter = new TextHighlighter
					{
						Ranges = { new TextRange(currentTextIndex, runTextLength) },
						Background = (background ?? Colors.Transparent).ToPlatform(),
						Foreground = textColor?.ToPlatform(),
					};

					textBlock.TextHighlighters.Add(textHighlighter);
				}

				currentTextIndex += runTextLength;
			}
		}

		public static IEnumerable<Tuple<Run, Color, Color>> ToRunAndColorsTuples(
			this FormattedString formattedString,
			IFontManager fontManager,
			double defaultLineHeight = 0d, // TODO: NET8 should be -1, but too late to change for net6
			TextAlignment defaultHorizontalAlignment = TextAlignment.Start,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
		{
			var runs = new List<Tuple<Run, Color, Color>>();

			if (formattedString != null && formattedString.Spans != null)
			{
				for (var i = 0; i < formattedString.Spans.Count; i++)
				{
					var span = formattedString.Spans[i];
					var run = span.ToRunAndColorsTuple(fontManager, defaultFont, defaultColor, defaultTextTransform);
					runs.Add(run);
				}
			}

			return runs;
		}

		public static Tuple<Run, Color, Color> ToRunAndColorsTuple(
			this Span span,
			IFontManager fontManager,
			Font? defaultFont = null,
			Color? defaultColor = null,
			TextTransform defaultTextTransform = TextTransform.Default)
		{
			var defaultFontSize = defaultFont?.Size ?? fontManager.DefaultFontSize;

			var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;

			var text = TextTransformUtilites.GetTransformedText(span.Text, transform);

			var run = new Run { Text = text ?? string.Empty };

			var font = span.ToFont(defaultFontSize);
			if (font.IsDefault && defaultFont.HasValue)
				font = defaultFont.Value;

			if (!font.IsDefault)
			{
				run.ApplyFont(font, fontManager);
			}

			if (span.IsSet(Span.TextDecorationsProperty))
				run.TextDecorations = (global::Windows.UI.Text.TextDecorations)span.TextDecorations;

			run.CharacterSpacing = span.CharacterSpacing.ToEm();

			return Tuple.Create(run, span.TextColor, span.BackgroundColor);
		}

		internal static void RecalculateSpanPositions(this TextBlock control, FormattedString formatted)
		{
			var spans = formatted.Spans;
			var labelWidth = control.ActualWidth;
			if (spans is null || spans.Count == 0 || labelWidth <= 0 || control.Height <= 0)
			{
				return;
			}

			for (int i = 0; i < spans.Count; i++)
			{
				var span = spans[i];

				var inline = control.Inlines.ElementAt(i);

				var startRect = inline.ContentStart.GetCharacterRect(LogicalDirection.Forward);
				var endRect = inline.ContentEnd.GetCharacterRect(LogicalDirection.Forward);

				var defaultLineHeight = GetMeasuredLineHeight(inline);
				var yaxis = startRect.Top;

				var lineHeights = new List<double>();

				while (yaxis < endRect.Bottom)
				{
					double lineHeight;
					if (yaxis == startRect.Top)
					{
						// First Line
						lineHeight = startRect.Bottom - startRect.Top;
					}
					else if (yaxis != endRect.Top)
					{
						// Middle Line(s)
						lineHeight = defaultLineHeight;
					}
					else
					{
						// Bottom Line
						lineHeight = endRect.Bottom - endRect.Top;
					}

					lineHeights.Add(lineHeight);
					yaxis += lineHeight;
				}

				var region = Region.FromLines(
					lineHeights.ToArray(),
					labelWidth,
					startRect.X,
					endRect.X + endRect.Width,
					startRect.Top);
				((ISpatialElement)span).Region = region.Inflate(10);
			}
		}
	}
}