#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Maui.Graphics;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Label;
using WSize = Windows.Foundation.Size;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class TextBlockExtensions
	{
		public static void RecalculateSpanPositions(this TextBlock control, Label element, IList<double> inlineHeights)
		{
			if (element?.FormattedText?.Spans == null
				|| element.FormattedText.Spans.Count == 0)
				return;

			var labelWidth = control.ActualWidth;

			if (labelWidth <= 0 || control.Height <= 0)
				return;

			for (int i = 0; i < element.FormattedText.Spans.Count; i++)
			{
				var span = element.FormattedText.Spans[i];

				var inline = control.Inlines.ElementAt(i);
				var rect = inline.ContentStart.GetCharacterRect(LogicalDirection.Forward);
				var endRect = inline.ContentEnd.GetCharacterRect(LogicalDirection.Forward);
				var defaultLineHeight = inlineHeights[i];

				var yaxis = rect.Top;
				var lineHeights = new List<double>();
				while (yaxis < endRect.Bottom)
				{
					double lineHeight;
					if (yaxis == rect.Top) // First Line
					{
						lineHeight = rect.Bottom - rect.Top;
					}
					else if (yaxis != endRect.Top) // Middle Line(s)
					{
						lineHeight = defaultLineHeight;
					}
					else // Bottom Line
					{
						lineHeight = endRect.Bottom - endRect.Top;
					}
					lineHeights.Add(lineHeight);
					yaxis += lineHeight;
				}

				((ISpatialElement)span).Region = Region.FromLines(lineHeights.ToArray(), labelWidth, rect.X, endRect.X + endRect.Width, rect.Top).Inflate(10);

			}
		}

		public static void UpdateLineBreakMode(this TextBlock textBlock, Label label) =>
			textBlock.UpdateLineBreakMode(label.LineBreakMode);

		public static void UpdateLineBreakMode(this TextBlock textBlock, LineBreakMode lineBreakMode)
		{
			if (textBlock == null)
				return;

			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					textBlock.TextTrimming = TextTrimming.Clip;
					textBlock.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.WordWrap:
					textBlock.TextTrimming = TextTrimming.None;
					textBlock.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.CharacterWrap:
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					textBlock.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.HeadTruncation:
					// TODO: This truncates at the end.
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					DetermineTruncatedTextWrapping(textBlock);
					break;
				case LineBreakMode.TailTruncation:
					textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
					DetermineTruncatedTextWrapping(textBlock);
					break;
				case LineBreakMode.MiddleTruncation:
					// TODO: This truncates at the end.
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					DetermineTruncatedTextWrapping(textBlock);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static void DetermineTruncatedTextWrapping(TextBlock textBlock) =>
			textBlock.TextWrapping = textBlock.MaxLines > 1 ? TextWrapping.Wrap : TextWrapping.NoWrap;

		public static void UpdateText(this TextBlock platformControl, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Html:
					platformControl.UpdateTextHtml(label);
					break;

				default:
					if (label.FormattedText != null)
						platformControl.UpdateInlines(label);
					else
						platformControl.Text = TextTransformUtilites.GetTransformedText(label.Text, label.TextTransform);
					break;
			}
		}

		public static double FindDefaultLineHeight(this TextBlock control, Inline inline)
		{
			control.Inlines.Add(inline);

			control.Measure(new WSize(double.PositiveInfinity, double.PositiveInfinity));

			var height = control.DesiredSize.Height;

			control.Inlines.Remove(inline);

			return height;
		}

		public static void UpdateMaxLines(this TextBlock platformControl, Label label)
		{
			if (label.MaxLines >= 0)
				platformControl.MaxLines = label.MaxLines;
			else
				platformControl.MaxLines = 0;
		}

		public static void UpdateDetectReadingOrderFromContent(this TextBlock platformControl, Label label)
		{
			if (label.IsSet(Specifics.DetectReadingOrderFromContentProperty))
				platformControl.SetTextReadingOrder(label.OnThisPlatform().GetDetectReadingOrderFromContent());
		}

		internal static void SetTextReadingOrder(this TextBlock platformControl, bool detectReadingOrderFromContent) =>
			platformControl.TextReadingOrder = detectReadingOrderFromContent
				? TextReadingOrder.DetectFromContent
				: TextReadingOrder.UseFlowDirection;
	}
}