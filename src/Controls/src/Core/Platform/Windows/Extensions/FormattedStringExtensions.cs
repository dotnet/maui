#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
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

			var runAndColorTuples = formattedString.ToRunAndColorsTuples(fontManager, defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform);

			var heights = new List<double>();
			int currentTextIndex = 0;
			foreach (var runAndColorTuple in runAndColorTuples)
			{
				Run run = runAndColorTuple.Item1;
				Color textColor = runAndColorTuple.Item2;
				Color background = runAndColorTuple.Item3;
				heights.Add(textBlock.FindDefaultLineHeight(run));
				textBlock.Inlines.Add(run);
				int length = run.Text.Length;

				if (background != null || textColor != null)
				{
					TextHighlighter textHighlighter = new TextHighlighter { Ranges = { new TextRange(currentTextIndex, length) } };

					if (background != null)
					{
						textHighlighter.Background = background.ToPlatform();
					}
					else
					{
						textHighlighter.Background = Colors.Transparent.ToPlatform();
					}

					if (textColor != null)
					{
						textHighlighter.Foreground = textColor.ToPlatform();
					}

					textBlock.TextHighlighters.Add(textHighlighter);
				}

				currentTextIndex += length;
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
	}
}