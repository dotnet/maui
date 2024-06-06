#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapDetectReadingOrderFromContent(LabelHandler handler, Label label) =>
			MapDetectReadingOrderFromContent((ILabelHandler)handler, label);

		public static void MapText(LabelHandler handler, Label label) =>
			MapText((ILabelHandler)handler, label);

		public static void MapDetectReadingOrderFromContent(ILabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, label);

		public static void MapText(ILabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapLineBreakMode(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateLineBreakMode(label);

		public static void MapMaxLines(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateMaxLines(label);

		protected override Size ArrangeOverride(Rect bounds)
		{
			var size = base.ArrangeOverride(bounds);

			RecalculateSpanPositions();

			return size;
		}

		void RecalculateSpanPositions()
		{
			if (Handler is not ILabelHandler labelHandler)
			{
				return;
			}

			var platformView = labelHandler.PlatformView;
			var virtualView = labelHandler.VirtualView as Label;
			if (platformView is null || virtualView is null)
			{
				return;
			}

			var formatted = virtualView.FormattedText;
			if (formatted is null)
			{
				return;
			}

			platformView.RecalculateSpanPositions(formatted);
		}
	}
}