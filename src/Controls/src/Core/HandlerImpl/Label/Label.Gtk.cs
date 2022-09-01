using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{

	public partial class Label
	{

		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateText(label, label.TextType);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateText(label, label.TextType);
		}

		public static void MapLineBreakMode(LabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateText(label, label.TextType);
		}

	}

}