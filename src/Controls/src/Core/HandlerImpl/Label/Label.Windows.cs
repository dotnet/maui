using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapDetectReadingOrderFromContent(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, label);

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
