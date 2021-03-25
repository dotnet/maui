using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.Extensions
{
	internal static class TextBlockExtensions
	{
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
	}
}
