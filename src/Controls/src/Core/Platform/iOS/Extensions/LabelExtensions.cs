#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this UILabel platformLabel, Label label)
		{
			switch (label.TextType)
			{
				case TextType.Html:
					platformLabel.UpdateTextHtml(label);
					break;

				default:
					if (label.FormattedText != null)

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
						platformLabel.AttributedText = label.ToNSAttributedString();
After:
					{
						platformLabel.AttributedText = label.ToNSAttributedString();
					}
*/
					{
						platformLabel.AttributedText = label.ToNSAttributedString();
					}
					else
					{
					{
						platformLabel.Text = TextTransformUtilites.GetTransformedText(label.Text, label.TextTransform);
					}

					break;
			}
		}
	}
}
