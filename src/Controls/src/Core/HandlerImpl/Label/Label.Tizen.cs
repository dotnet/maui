using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);

		public static void MapTextType(ILabelHandler handler, Label label)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		[MissingMapper]
		public static void MapMaxLines(ILabelHandler handler, Label label) { }
	}
}
