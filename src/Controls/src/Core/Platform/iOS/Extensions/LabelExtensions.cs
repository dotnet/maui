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
			var text = TextTransformUtilities.GetTransformedText(label.Text, label.TextTransform);

			switch (label.TextType)
			{
				case TextType.Html:
					// NOTE: Setting HTML text this will crash with some sort of consistency error.
					// https://github.com/dotnet/maui/issues/25946
					// Here we have to dispatch back the the main queue to avoid the crash.
					// This is observed with CarouselView 1 but not with 2, so hopefully this
					// will be just disappear once we switch.
					CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						platformLabel.UpdateTextHtml(text);

						if (label.Handler is LabelHandler labelHandler)
							Label.MapFormatting(labelHandler, label);

						// NOTE: Because we are updating text outside the normal layout
						// pass, we need to invalidate the measure for the next pass.
						label.InvalidateMeasure();
					});
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

						platformLabel.Text = text;
					}
					break;
			}
		}
	}
}
