using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.UI.Text;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static void UpdateText(this TextBlock textBlock, FormattedString formatted)
		{
			textBlock.Inlines.Clear();
			// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
			textBlock.Measure(new global::Windows.Foundation.Size(double.MaxValue, double.MaxValue));

			var heights = new List<double>();
			for (var i = 0; i < formatted.Spans.Count; i++)
			{
				var span = formatted.Spans[i];

				var run = span.ToRun();
				heights.Add(Control.FindDefaultLineHeight(run));
				textBlock.Inlines.Add(run);
			}
			_inlineHeights = heights;
		}

		public static Run ToRun(this Span span)
		{
			var run = new Run { Text = span.Text ?? string.Empty };

			if (span.TextColor.IsNotDefault())
				run.Foreground = span.TextColor.ToNative();

			if (!span.IsDefault())
				run.ApplyFont(span);

			if (span.IsSet(Span.TextDecorationsProperty))
				run.TextDecorations = (global::Windows.UI.Text.TextDecorations)span.TextDecorations;

			run.CharacterSpacing = span.CharacterSpacing.ToEm();

			return run;
		}
	}
}