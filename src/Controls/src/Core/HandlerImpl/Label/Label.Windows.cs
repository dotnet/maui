namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapDetectReadingOrderFromContent(LabelHandler handler, Label label)
		{
			Platform.InputViewExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, label);
		}

		public static void MapTextType(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapText(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);
	}
}
