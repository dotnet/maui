using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapLineBreakMode(LabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}
	}
}
