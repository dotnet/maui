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
#pragma warning disable CS0618 // Type or member is obsolete
			switch (label.TextType)
			{
				case TextType.Html:
#pragma warning restore CS0618 // Type or member is obsolete
					platformLabel.UpdateTextHtml(label);
					break;

				default:
					if (label.FormattedText != null)
						platformLabel.AttributedText = label.ToNSAttributedString();
					else
					{
						if (platformLabel.AttributedText is not null)
						{
							platformLabel.AttributedText = null;
						}

						platformLabel.Text = TextTransformUtilities.GetTransformedText(label.Text, label.TextTransform);
					}
					break;
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
