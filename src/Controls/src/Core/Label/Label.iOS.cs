#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		protected override Size ArrangeOverride(Rect bounds)
		{
			var size = base.ArrangeOverride(bounds);

			// On iOS 26+ with NavigationPage, the UILabel's native Bounds may not be
			// finalized during MAUI's ArrangeOverride due to the WrapperView layout
			// timing. Detect this by checking if the UILabel Bounds are unset despite
			// a valid MAUI-computed size, and defer span recalculation to the next main
			// run loop iteration when iOS has propagated the frame correctly.
			if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)) && HasFormattedTextSpans && Handler is LabelHandler labelHandler &&
				labelHandler.PlatformView is UILabel platformLabel &&
				platformLabel.Bounds.Width == 0 && size.Width > 0)
			{
				platformLabel.BeginInvokeOnMainThread(() =>
				{
					var bounds = platformLabel.Bounds;
					if (bounds.Width > 0 && bounds.Height > 0)
					{
						RecalculateSpanPositions(new Size(bounds.Width, bounds.Height));
					}
				});
			}
			else
			{
				RecalculateSpanPositions(size);
			}

			return size;
		}

		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.LabelExtensions.UpdateText(handler.PlatformView, label);

			MapFormatting(handler, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}

		internal static void MapFormatting(ILabelHandler handler, Label label)
		{
			if (IsPlainText(label))
			{
				LabelHandler.MapFormatting(handler, label);
			}
			else if (!label.HasFormattedTextSpans && label.TextType == TextType.Html) // we need to re-apply color and font for HTML labels
			{
				handler.UpdateValue(nameof(ILabel.TextColor));
				handler.UpdateValue(nameof(ILabel.Font));
			}
		}

		void RecalculateSpanPositions(Size size)
		{
			if (Handler is LabelHandler labelHandler)
			{
				if (labelHandler.PlatformView is not UILabel platformView || labelHandler.VirtualView is not Label virtualView)
					return;

				platformView.RecalculateSpanPositions(virtualView, size);
			}
		}
	}
}