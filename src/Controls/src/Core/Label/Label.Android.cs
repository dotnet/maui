#nullable disable
using System;
using Android.Text;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label) => MapTextType((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapLineBreakMode(LabelHandler handler, Label label) => MapLineBreakMode((ILabelHandler)handler, label);


		public static void MapTextType(ILabelHandler handler, Label label)
		{
			handler.UpdateValue(nameof(ILabel.Text));
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}

		internal static void MapFrame(ILabelHandler handler, Label label, object args)
		{
			// don't attempt to layout if this is not a formatted text WITH some text
			if (label.TextType != TextType.Text || label.FormattedText is not FormattedString text || text?.Spans == null || text.Spans.Count == 0)
				return;

			handler.PlatformView?.RecalculateSpanPositions(label.Padding, text);
		}
	}
}
