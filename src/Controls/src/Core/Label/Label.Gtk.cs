using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{

	public partial class Label
	{

		public static void MapText(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateText(label, label.TextType);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label.LineBreakMode, label.MaxLines);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label.MaxLines);
		}

	}

}