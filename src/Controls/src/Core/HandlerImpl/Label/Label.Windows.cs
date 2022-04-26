using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapText(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapLineBreakMode(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateLineBreakMode(label.LineBreakMode);

		public static void MapMaxLines(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateMaxLines(label);
	}
}
