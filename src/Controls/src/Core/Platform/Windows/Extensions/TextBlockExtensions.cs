using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Label;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class TextBlockExtensions
	{
		public static void UpdateLineBreakMode(this TextBlock textBlock, Label label) =>
			textBlock.SetLineBreakMode(label.LineBreakMode, label.MaxLines);

		public static void UpdateLineBreakMode(this TextBlock textBlock, LineBreakMode lineBreakMode)
		{
			textBlock.SetLineBreakMode(lineBreakMode, null);
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
					{
						if (platformControl.TextHighlighters.Count > 0)
						{
							platformControl.TextHighlighters.Clear();
						}
						platformControl.Text = TextTransformUtilites.GetTransformedText(label.Text, label.TextTransform);
					}
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
			// Linebreak mode also handles setting MaxLines
			platformControl.SetLineBreakMode(label.LineBreakMode, label.MaxLines);
		}

		public static void UpdateDetectReadingOrderFromContent(this TextBlock platformControl, Label label)
		{
			if (label.IsSet(Specifics.DetectReadingOrderFromContentProperty))
				platformControl.SetTextReadingOrder(label.OnThisPlatform().GetDetectReadingOrderFromContent());
		}

		internal static void SetLineBreakMode(this TextBlock textBlock, LineBreakMode lineBreakMode, int? maxLines = null)
		{
			if (maxLines.HasValue && maxLines >= 0)
				textBlock.MaxLines = maxLines.Value;
			else
				textBlock.MaxLines = 0;

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

		internal static void SetTextReadingOrder(this TextBlock platformControl, bool detectReadingOrderFromContent) =>
			platformControl.TextReadingOrder = detectReadingOrderFromContent
				? TextReadingOrder.DetectFromContent
				: TextReadingOrder.UseFlowDirection;
	}
}