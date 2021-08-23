using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.NativeView, label);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.NativeView, label);
		}
	}
}
