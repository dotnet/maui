#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
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
					// NOTE: Setting HTML text like this will crash with some sort of consistency error.
					// when inside a CV1 (CollectionView/CarouselView handler v1) layout pass.
					// https://github.com/dotnet/maui/issues/25946
					// CV2 (the default handler in .NET 10) does NOT have this crash.
					if (IsPlatformLabelInsideCV2Cell(platformLabel))
					{
						// Synchronous: safe in CV2, avoids the two-pass jitter.
						// No InvalidateMeasure needed — text is already correct when measured.
						platformLabel.UpdateTextHtml(text);

						if (label.Handler is LabelHandler labelHandlerSync)
						{
							Label.MapFormatting(labelHandlerSync, label);
						}
					}
					else
					{
						CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
						{
							platformLabel.UpdateTextHtml(text);

							if (label.Handler is LabelHandler labelHandler)
							{
								Label.MapFormatting(labelHandler, label);
							}

							// NOTE: Because we are updating text outside the normal layout
							// pass, we need to invalidate the measure for the next pass.
							label.InvalidateMeasure();
						});
					}
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

		// Walks the native UIKit superview chain to determine if this UILabel lives inside a CV2 cell.
		static bool IsPlatformLabelInsideCV2Cell(UILabel platformLabel)
		{
			var superview = platformLabel.Superview;
			while (superview is not null)
			{
				if (superview is ItemsViewCell2)
				{
					return true;
				}
				superview = superview.Superview;
			}
			return false;
		}
	}
}
