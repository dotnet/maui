#nullable enable
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static void UpdateInlines(this TextBlock textBlock, Label label)
			=> UpdateInlines(textBlock, label.FormattedText, label.LineHeight, label.HorizontalTextAlignment, label.ToFont(), label.TextColor, label.TextTransform);

		public static void UpdateInlines(this TextBlock textBlock, FormattedString formattedString, double defaultLineHeight = 0d, TextAlignment defaultHorizontalAlignment = TextAlignment.Start, Font? defaultFont = null, Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default)
		{
			textBlock.Inlines.Clear();
			// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
			textBlock.Measure(new global::Windows.Foundation.Size(double.MaxValue, double.MaxValue));

			var runs = formattedString.ToRuns(defaultLineHeight, defaultHorizontalAlignment, defaultFont, defaultColor, defaultTextTransform);

			var heights = new List<double>();
			foreach (var run in runs)
			{
				heights.Add(textBlock.FindDefaultLineHeight(run));
				textBlock.Inlines.Add(run);
			}
		}

		public static IEnumerable<Run> ToRuns(this FormattedString formattedString, double defaultLineHeight = 0d, TextAlignment defaultHorizontalAlignment = TextAlignment.Start, Font? defaultFont = null, Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default, IFontManager? fontManager = null)
		{
			var runs = new List<Run>();

			if (formattedString != null && formattedString.Spans != null)
			{
				fontManager = fontManager ?? formattedString.RequireFontManager();

				for (var i = 0; i < formattedString.Spans.Count; i++)
				{
					var span = formattedString.Spans[i];
					var run = span.ToRun(fontManager, defaultFont, defaultColor, defaultTextTransform);
					runs.Add(run);
				}
			}

			return runs;
		}

		public static Run ToRun(this Span span, IFontManager fontManager, Font? defaultFont = null, Color? defaultColor = null, TextTransform defaultTextTransform = TextTransform.Default)
		{
			var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;

			var text = TextTransformUtilites.GetTransformedText(span.Text, transform);
			
			var run = new Run { Text = text ?? string.Empty };

			var fgcolor = span.TextColor ?? defaultColor;
			if (fgcolor is not null)
				run.Foreground = fgcolor.ToNative();

			// NOTE: Background is not supported in Run

			var font = span.ToFont();
			if (font.IsDefault && defaultFont.HasValue)
				font = defaultFont.Value;

			if (!font.IsDefault)
			{
				run.ApplyFont(font, fontManager);
			}

			if (span.IsSet(Span.TextDecorationsProperty))
				run.TextDecorations = (global::Windows.UI.Text.TextDecorations)span.TextDecorations;

			run.CharacterSpacing = span.CharacterSpacing.ToEm();

			return run;
		}
	}
}