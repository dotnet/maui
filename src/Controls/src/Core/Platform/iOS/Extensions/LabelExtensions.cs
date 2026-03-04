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
					// NOTE: Setting HTML text this will crash with some sort of consistency error
					// when inside a CV1 (CollectionView/CarouselView handler v1) layout pass.
					// https://github.com/dotnet/maui/issues/25946
					// CV2 (the default handler in .NET 10) does NOT have this crash.
					// Applying HTML synchronously in CV2 prevents the jitter caused by the
					// deferred measurement (measure with no text → HTML applied → remeasure).
					// https://github.com/dotnet/maui/issues/33065
					if (IsLabelInsideCV2Handler(label))
					{
						// Synchronous: inside a CV2 layout pass — no InvalidateMeasure needed.
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

		// Walks the MAUI logical parent tree to determine if this Label lives inside a CV2 handler
		static bool IsLabelInsideCV2Handler(Label label)
		{
			var parent = label.Parent;
			while (parent is not null)
			{
				if (parent is ItemsView itemsView)
				{
					return itemsView.Handler is CollectionViewHandler2;
				}
				parent = parent.Parent;
			}
			return false;
		}
	}
}
